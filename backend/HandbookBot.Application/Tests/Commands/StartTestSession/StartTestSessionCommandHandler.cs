using HandbookBot.Application.Common.Exceptions;
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
        var user = await uow.Users.GetByTelegramIdAsync(request.UserId, ct)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (request.Mode == TestMode.AI && string.IsNullOrEmpty(user.OpenAiApiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Go to Settings to add it.");

        var topics = await CollectTopicsAsync(request, ct);
        if (topics.Count == 0) throw new InvalidOperationException("No topics found for the selected scope.");

        var session = TestSession.Create(request.UserId, request.Mode, request.SectionId, request.SubsectionId, request.TopicId);
        await uow.TestSessions.AddAsync(session, ct);

        var results = new List<TestResult>();
        int order = 0;

        foreach (var topic in topics)
        {
            if (request.Mode == TestMode.AI)
            {
                var qas = await aiService.GenerateQuestionsAsync(topic.Title, topic.Content, request.QuestionsPerTopic, user.OpenAiApiKey!, ct);
                results.AddRange(qas.Select(qa => TestResult.Create(session.Id, topic.Id, qa.Question, qa.Answer, order++)));
            }
            else
            {
                var question = string.IsNullOrWhiteSpace(topic.Summary)
                    ? $"Расскажи о теме: {topic.Title}"
                    : topic.Summary;
                results.Add(TestResult.Create(session.Id, topic.Id, question, topic.Content, order++));
            }
        }

        session.SetTotalQuestions(results.Count);
        await uow.TestResults.AddRangeAsync(results, ct);
        await uow.SaveChangesAsync(ct);

        return new TestSessionDto(session.Id, session.Mode, session.TotalQuestions, 0, 0, false, session.CreatedAt, null);
    }

    private async Task<IReadOnlyList<Topic>> CollectTopicsAsync(StartTestSessionCommand req, CancellationToken ct)
    {
        if (req.TopicId.HasValue)
        {
            var t = await uow.Topics.GetByIdAsync(req.TopicId.Value, ct);
            return t is null ? [] : [t];
        }
        if (req.SubsectionId.HasValue) return await uow.Topics.GetRootsBySubsectionIdAsync(req.SubsectionId.Value, ct);
        if (req.SectionId.HasValue) return await uow.Topics.GetBySectionIdAsync(req.SectionId.Value, ct);

        var sections = await uow.Sections.GetByUserIdAsync(req.UserId, ct);
        var all = new List<Topic>();
        foreach (var s in sections)
            all.AddRange(await uow.Topics.GetBySectionIdAsync(s.Id, ct));
        return all;
    }
}
