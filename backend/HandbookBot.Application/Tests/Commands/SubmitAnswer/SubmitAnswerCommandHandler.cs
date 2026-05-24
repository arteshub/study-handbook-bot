using System.Text.Json;
using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.SubmitAnswer;

internal sealed class SubmitAnswerCommandHandler(IUnitOfWork uow)
    : IRequestHandler<SubmitAnswerCommand, TestAnswerResultDto>
{
    public async Task<TestAnswerResultDto> Handle(SubmitAnswerCommand request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();
        if (session.IsCompleted) throw new InvalidOperationException("Session is already completed.");

        var result = session.Results.FirstOrDefault(r => r.Id == request.ResultId)
            ?? throw new NotFoundException(nameof(TestResult), request.ResultId);

        bool isCorrect;
        IReadOnlyList<AnswerOptionResultDto>? optionResults = null;

        if (result.OptionsJson is not null)
        {
            // Multiple choice (AI mode)
            var options = JsonSerializer.Deserialize<List<StoredOption>>(result.OptionsJson, JsonOpts) ?? [];
            var selected = options.FirstOrDefault(o => o.i == (request.SelectedOptionIndex ?? 0));
            isCorrect = selected?.IsCorrect ?? false;
            optionResults = options.Select(o => new AnswerOptionResultDto(o.i, o.Text, o.IsCorrect, o.Explanation)).ToList();
            result.SubmitAnswer(request.SelectedOptionIndex?.ToString(), isCorrect, null);
        }
        else
        {
            // Self mode
            isCorrect = request.SelfMarkedCorrect ?? false;
            result.SubmitAnswer(request.UserAnswer, isCorrect, null);
        }

        if (isCorrect) session.RecordCorrectAnswer();

        // Update SRS progress for this topic
        var progress = await uow.TopicProgress.GetByUserAndTopicAsync(session.UserId, result.TopicId, ct);
        if (progress is null)
        {
            progress = TopicProgress.Create(session.UserId, result.TopicId);
            progress.RecordReview(isCorrect);
            await uow.TopicProgress.AddAsync(progress, ct);
        }
        else
        {
            progress.RecordReview(isCorrect);
            uow.TopicProgress.Update(progress);
        }

        uow.TestSessions.Update(session);
        await uow.SaveChangesAsync(ct);

        int answered = session.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);

        return new TestAnswerResultDto(isCorrect, result.CorrectAnswer, null, session.CorrectAnswers, answered, optionResults);
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private record StoredOption(int i, string Text, bool IsCorrect, string Explanation);
}
