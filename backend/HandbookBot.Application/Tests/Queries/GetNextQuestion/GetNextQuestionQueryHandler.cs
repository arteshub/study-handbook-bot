using System.Text.Json;
using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetNextQuestion;

internal sealed class GetNextQuestionQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetNextQuestionQuery, TestQuestionDto?>
{
    public async Task<TestQuestionDto?> Handle(GetNextQuestionQuery request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();
        if (session.IsCompleted) return null;

        var next = session.Results
            .Where(r => r.UserAnswer == null && r.IsCorrect == null)
            .OrderBy(r => r.Order)
            .FirstOrDefault();

        if (next is null) return null;

        var topic = await uow.Topics.GetByIdAsync(next.TopicId, ct)
            ?? throw new NotFoundException(nameof(Topic), next.TopicId);

        int answered = session.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);

        IReadOnlyList<QuestionOptionDto>? options = null;
        if (next.OptionsJson is not null)
        {
            var raw = JsonSerializer.Deserialize<List<StoredOption>>(next.OptionsJson, JsonOpts) ?? [];
            options = raw.Select(o => new QuestionOptionDto(o.i, o.Text)).ToList();
        }

        // For self mode, include the correct answer so the frontend can show it before submission
        string? correctAnswer = next.OptionsJson is null ? next.CorrectAnswer : null;

        return new TestQuestionDto(
            next.Id,
            next.TopicId,
            topic.Title,
            next.Question,
            correctAnswer,
            answered + 1,
            session.TotalQuestions,
            options,
            next.CachedQuestionId);
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private record StoredOption(int i, string Text, bool IsCorrect, string Explanation);
}
