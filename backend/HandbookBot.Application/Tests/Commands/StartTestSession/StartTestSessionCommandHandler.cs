using System.Text.Json;
using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.StartTestSession;

internal sealed class StartTestSessionCommandHandler(IUnitOfWork uow, IAiService aiService)
    : IRequestHandler<StartTestSessionCommand, TestSessionDto>
{
    public async Task<TestSessionDto> Handle(StartTestSessionCommand request, CancellationToken ct)
    {
        var topics = await CollectTopicsAsync(request, ct);
        if (topics.Count == 0) throw new InvalidOperationException("No topics found for the selected scope.");

        var session = TestSession.Create(request.UserId, request.Mode,
            request.SectionIds is { Count: 1 } ? request.SectionIds[0] : null,
            request.SubsectionIds is { Count: 1 } ? request.SubsectionIds[0] : null,
            request.TopicIds is { Count: 1 } ? request.TopicIds[0] : null);
        await uow.TestSessions.AddAsync(session, ct);

        var results = new List<TestResult>();
        int order = 0;

        foreach (var topic in topics)
        {
            if (string.IsNullOrWhiteSpace(topic.Content)) continue;

            int count = QuestionsCountForContent(topic.Content, request.QuestionsPerTopic);

            if (request.Mode == TestMode.AI)
            {
                var qas = await aiService.GenerateQuestionsAsync(topic.Title, topic.Content, count, ct);
                foreach (var (question, options) in qas)
                {
                    var optionsJson = JsonSerializer.Serialize(options.Select((o, i) => new { i, o.Text, o.IsCorrect, o.Explanation }));
                    results.Add(TestResult.Create(session.Id, topic.Id, question, string.Empty, order++, optionsJson));
                }
            }
            else
            {
                var qas = await aiService.GenerateSelfTestQuestionsAsync(topic.Title, topic.Content, count, ct);
                results.AddRange(qas.Select(qa => TestResult.Create(session.Id, topic.Id, qa.Question, qa.ModelAnswer, order++)));
            }
        }

        session.SetTotalQuestions(results.Count);
        await uow.TestResults.AddRangeAsync(results, ct);
        await uow.SaveChangesAsync(ct);

        return new TestSessionDto(session.Id, session.Mode, session.TotalQuestions, 0, 0, false, session.CreatedAt, null);
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

    private static int QuestionsCountForContent(string content, int _)
    {
        var len = content.Length;
        if (len < 500)  return 5;
        if (len < 2000) return 7;
        if (len < 5000) return 9;
        return 12;
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
