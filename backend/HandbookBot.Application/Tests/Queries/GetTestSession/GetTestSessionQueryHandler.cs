using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetTestSession;

internal sealed class GetTestSessionQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTestSessionQuery, TestSessionDto>
{
    public async Task<TestSessionDto> Handle(GetTestSessionQuery request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();

        int answered = session.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);

        return new TestSessionDto(
            session.Id,
            session.Mode,
            session.TotalQuestions,
            session.CorrectAnswers,
            answered,
            session.IsCompleted,
            session.CreatedAt,
            session.CompletedAt);
    }
}
