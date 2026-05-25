using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HandbookBot.Application.Tests.Commands.StartTestSession;

internal sealed class StartTestSessionCommandHandler(IUnitOfWork uow, IAiService aiService, IServiceScopeFactory scopeFactory)
    : IRequestHandler<StartTestSessionCommand, TestSessionDto>
{
    public async Task<TestSessionDto> Handle(StartTestSessionCommand request, CancellationToken ct)
    {
        var session = TestSession.Create(request.UserId, request.Mode,
            request.SectionIds is { Count: 1 } ? request.SectionIds[0] : null,
            request.SubsectionIds is { Count: 1 } ? request.SubsectionIds[0] : null,
            request.TopicIds is { Count: 1 } ? request.TopicIds[0] : null);
        await uow.TestSessions.AddAsync(session, ct);

        List<TestResult> results;

        if (request.WrongAnswersMode)
        {
            results = await BuildWrongAnswerResultsAsync(session, request.UserId, ct);
        }
        else
        {
            var topics = await CollectTopicsAsync(request, ct);
            if (topics.Count == 0) throw new InvalidOperationException("No topics found for the selected scope.");
            results = await BuildNormalResultsAsync(session, topics, request.Mode, ct);
        }

        session.SetTotalQuestions(results.Count);
        await uow.TestResults.AddRangeAsync(results, ct);
        await uow.SaveChangesAsync(ct);

        return new TestSessionDto(session.Id, session.Mode, session.TotalQuestions, 0, 0, false, session.CreatedAt, null);
    }

    private async Task<List<TestResult>> BuildWrongAnswerResultsAsync(TestSession session, long userId, CancellationToken ct)
    {
        var topicIds = await uow.TestResults.GetTopicsWithWrongAnswersAsync(userId, ct);

        var pool = new List<(Guid TopicId, string Question, string ModelAnswer)>();
        foreach (var topicId in topicIds)
        {
            var cached = await uow.CachedQuestions.GetAllByTopicAsync(topicId, TestMode.Self, ct);
            foreach (var q in cached)
                pool.Add((topicId, q.QuestionText, q.ModelAnswer ?? string.Empty));
        }

        int order = 0;
        return pool
            .OrderBy(_ => Guid.NewGuid())
            .Select(x => TestResult.Create(session.Id, x.TopicId, x.Question, x.ModelAnswer, order++))
            .ToList();
    }

    private async Task<List<TestResult>> BuildNormalResultsAsync(TestSession session, IReadOnlyList<Topic> topics, TestMode mode, CancellationToken ct)
    {
        using var aiCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        var aiCt = aiCts.Token;

        var results = new List<TestResult>();
        int order = 0;

        foreach (var topic in topics)
        {
            if (string.IsNullOrWhiteSpace(topic.Content)) continue;

            var contentHash = ComputeHash(topic.Content);
            int count = QuestionsCountForContent(topic.Content, mode);

            if (mode == TestMode.AI)
            {
                var cached = await uow.CachedQuestions.GetAsync(topic.Id, TestMode.AI, contentHash, ct);
                IReadOnlyList<(string Question, IReadOnlyList<AiOption> Options)> qas;

                if (cached.Count > 0)
                {
                    qas = cached
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(count)
                        .Select(q => ((string)q.QuestionText, (IReadOnlyList<AiOption>)DeserializeOptions(q.OptionsJson!)))
                        .ToList();
                }
                else
                {
                    qas = await aiService.GenerateQuestionsAsync(topic.Title, topic.Content, count, aiCt);

                    var toCache = qas.Select(q =>
                        CachedQuestion.CreateAi(topic.Id, contentHash, q.Question,
                            JsonSerializer.Serialize(q.Options.Select((o, i) => new { i, o.Text, o.IsCorrect, o.Explanation }))))
                        .ToList();
                    _ = SaveCacheAsync(topic.Id, TestMode.AI, contentHash, toCache, aiCt);
                }

                foreach (var (question, options) in qas)
                {
                    var optionsJson = JsonSerializer.Serialize(options.Select((o, i) => new { i, o.Text, o.IsCorrect, o.Explanation }));
                    results.Add(TestResult.Create(session.Id, topic.Id, question, string.Empty, order++, optionsJson));
                }
            }
            else
            {
                var cached = await uow.CachedQuestions.GetAsync(topic.Id, TestMode.Self, contentHash, ct);
                IReadOnlyList<(string Question, string ModelAnswer)> qas;

                if (cached.Count > 0)
                {
                    qas = cached
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(count)
                        .Select(q => (q.QuestionText, q.ModelAnswer ?? string.Empty))
                        .ToList();
                }
                else
                {
                    qas = await aiService.GenerateSelfTestQuestionsAsync(topic.Title, topic.Content, count, aiCt);

                    var toCache = qas.Select(q => CachedQuestion.CreateSelf(topic.Id, contentHash, q.Question, q.ModelAnswer)).ToList();
                    _ = SaveCacheAsync(topic.Id, TestMode.Self, contentHash, toCache, aiCt);
                }

                results.AddRange(qas.Select(qa => TestResult.Create(session.Id, topic.Id, qa.Question, qa.ModelAnswer, order++)));
            }
        }

        return results;
    }

    // Saves generated questions to cache in a new DI scope,
    // independent of the HTTP request scope (which may be disposed on client timeout).
    private async Task SaveCacheAsync(Guid topicId, TestMode mode, string contentHash, IReadOnlyList<CachedQuestion> questions, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var cacheUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await cacheUow.CachedQuestions.DeleteStaleAsync(topicId, mode, contentHash, ct);
            await cacheUow.CachedQuestions.AddRangeAsync(questions, ct);
            await cacheUow.SaveChangesAsync(ct);
        }
        catch { /* cache save is best-effort; next request will regenerate */ }
    }

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static IReadOnlyList<AiOption> DeserializeOptions(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.EnumerateArray()
            .Select(el => new AiOption(
                el.GetProperty("Text").GetString()!,
                el.GetProperty("IsCorrect").GetBoolean(),
                el.GetProperty("Explanation").GetString()!))
            .ToList();
    }

    private async Task<IReadOnlyList<Topic>> CollectTopicsAsync(StartTestSessionCommand req, CancellationToken ct)
    {
        if (req.ReviewMode)
        {
            var due = await uow.TopicProgress.GetDueForReviewAsync(req.UserId, ct);
            var reviewed = new List<Topic>();
            foreach (var p in due)
            {
                var t = await uow.Topics.GetByIdAsync(p.TopicId, ct);
                if (t is not null && !string.IsNullOrWhiteSpace(t.Content)) reviewed.Add(t);
            }
            return reviewed;
        }

        if (req.TopicIds?.Count > 0)
        {
            var result = new List<Topic>();
            foreach (var id in req.TopicIds)
                await CollectRecursiveAsync(id, result, ct);
            return result.DistinctBy(t => t.Id).ToList();
        }
        if (req.SubsectionIds?.Count > 0)
        {
            var result = new List<Topic>();
            foreach (var id in req.SubsectionIds)
                result.AddRange(await uow.Topics.GetAllBySubsectionIdAsync(id, ct));
            return result.DistinctBy(t => t.Id).ToList();
        }
        if (req.SectionIds?.Count > 0)
        {
            var result = new List<Topic>();
            foreach (var id in req.SectionIds)
                result.AddRange(await uow.Topics.GetBySectionIdAsync(id, ct));
            return result.DistinctBy(t => t.Id).ToList();
        }

        var sections = await uow.Sections.GetByUserIdAsync(req.UserId, ct);
        var all = new List<Topic>();
        foreach (var s in sections)
            all.AddRange(await uow.Topics.GetBySectionIdAsync(s.Id, ct));
        return all;
    }

    private static int QuestionsCountForContent(string content, TestMode mode)
    {
        var len = content.Length;
        if (mode == TestMode.AI)
        {
            if (len < 500)   return 15;
            if (len < 2000)  return 21;
            if (len < 5000)  return 28;
            return 35;
        }
        // Self mode — Q+A is much more token-efficient, so higher counts are feasible
        if (len < 500)   return 21;
        if (len < 2000)  return 30;
        if (len < 5000)  return 50;
        if (len < 10000) return 70;
        return 100;
    }

    private async Task CollectRecursiveAsync(Guid topicId, List<Topic> result, CancellationToken ct)
    {
        var topic = await uow.Topics.GetByIdAsync(topicId, ct);
        if (topic is not null) result.Add(topic);

        var children = await uow.Topics.GetChildrenAsync(topicId, ct);
        foreach (var child in children)
            await CollectRecursiveAsync(child.Id, result, ct);
    }

}
