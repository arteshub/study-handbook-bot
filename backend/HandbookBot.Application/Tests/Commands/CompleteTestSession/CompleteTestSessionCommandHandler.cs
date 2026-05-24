using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.CompleteTestSession;

internal sealed class CompleteTestSessionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CompleteTestSessionCommand, TestSessionDto>
{
    public async Task<TestSessionDto> Handle(CompleteTestSessionCommand request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();

        if (!session.IsCompleted) session.Complete();

        uow.TestSessions.Update(session);
        await uow.SaveChangesAsync(ct);

        int answered = session.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);

        return new TestSessionDto(session.Id, session.Mode, session.TotalQuestions, session.CorrectAnswers, answered, true, session.CreatedAt, session.CompletedAt);
    }
}
