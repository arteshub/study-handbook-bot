using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.SubmitAnswer;

internal sealed class SubmitAnswerCommandHandler(IUnitOfWork uow, IAiService aiService)
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
        string? feedback = null;

        if (session.Mode == TestMode.Self)
        {
            isCorrect = request.SelfMarkedCorrect ?? false;
            result.SubmitAnswer(request.UserAnswer, isCorrect, null);
        }
        else
        {
            var user = await uow.Users.GetByTelegramIdAsync(request.UserId, ct)!;
            var evaluation = await aiService.EvaluateAnswerAsync(
                result.Question,
                result.CorrectAnswer,
                request.UserAnswer ?? string.Empty,
                user!.OpenAiApiKey!,
                ct);

            isCorrect = evaluation.IsCorrect;
            feedback = evaluation.Feedback;
            result.SubmitAnswer(request.UserAnswer, isCorrect, feedback);
        }

        if (isCorrect) session.RecordCorrectAnswer();

        uow.TestSessions.Update(session);
        await uow.SaveChangesAsync(ct);

        int answered = session.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);

        return new TestAnswerResultDto(isCorrect, result.CorrectAnswer, feedback, session.CorrectAnswers, answered);
    }
}
