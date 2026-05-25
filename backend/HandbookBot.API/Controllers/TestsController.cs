using HandbookBot.Application.Review.Queries.GetReviewStats;
using HandbookBot.Application.Tests.Commands.CompleteTestSession;
using HandbookBot.Application.Tests.Commands.SkipQuestion;
using HandbookBot.Application.Tests.Commands.SkipTopic;
using HandbookBot.Application.Tests.Commands.StartTestSession;
using HandbookBot.Application.Tests.Commands.SubmitAnswer;
using HandbookBot.Application.Tests.Queries.GetNextQuestion;
using HandbookBot.Application.Tests.Queries.GetTestHistory;
using HandbookBot.Application.Tests.Queries.GetTestSession;
using HandbookBot.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class TestsController : BaseController
{
    [HttpGet("review/stats")]
    public async Task<IActionResult> GetReviewStats(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetReviewStatsQuery(CurrentUserId), ct));

    [HttpPost("sessions")]
    public async Task<IActionResult> Start([FromBody] StartTestRequest req, CancellationToken ct)
    {
        var session = await Mediator.Send(new StartTestSessionCommand(
            CurrentUserId, req.Mode, req.SectionIds, req.SubsectionIds, req.TopicIds, req.ReviewMode, req.QuestionsPerTopic), ct);
        return Created($"/api/tests/sessions/{session.Id}", session);
    }

    [HttpGet("sessions/{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTestSessionQuery(id, CurrentUserId), ct));

    [HttpGet("sessions/{id:guid}/next")]
    public async Task<IActionResult> GetNextQuestion(Guid id, CancellationToken ct)
    {
        var question = await Mediator.Send(new GetNextQuestionQuery(id, CurrentUserId), ct);
        return question is null ? NoContent() : Ok(question);
    }

    [HttpPost("sessions/{id:guid}/answers")]
    public async Task<IActionResult> SubmitAnswer(Guid id, [FromBody] SubmitAnswerRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new SubmitAnswerCommand(
            id, req.ResultId, CurrentUserId, req.UserAnswer, req.SelfMarkedCorrect, req.SelectedOptionIndex), ct);
        return Ok(result);
    }

    [HttpPost("sessions/{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct) =>
        Ok(await Mediator.Send(new CompleteTestSessionCommand(id, CurrentUserId), ct));

    [HttpPost("sessions/{id:guid}/skip-topic/{topicId:guid}")]
    public async Task<IActionResult> SkipTopic(Guid id, Guid topicId, CancellationToken ct)
    {
        await Mediator.Send(new SkipTopicCommand(id, topicId, CurrentUserId), ct);
        return NoContent();
    }

    [HttpPost("sessions/{id:guid}/skip-question/{resultId:guid}")]
    public async Task<IActionResult> SkipQuestion(Guid id, Guid resultId, CancellationToken ct)
    {
        await Mediator.Send(new SkipQuestionCommand(id, resultId, CurrentUserId), ct);
        return NoContent();
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTestHistoryQuery(CurrentUserId), ct));
}

public sealed record StartTestRequest(
    TestMode Mode,
    List<Guid>? SectionIds,
    List<Guid>? SubsectionIds,
    List<Guid>? TopicIds,
    bool ReviewMode = false,
    int QuestionsPerTopic = 3);

public sealed record SubmitAnswerRequest(
    Guid ResultId,
    string? UserAnswer,
    bool? SelfMarkedCorrect,
    int? SelectedOptionIndex);
