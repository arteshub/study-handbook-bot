using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.SkipQuestion;

internal sealed class SkipQuestionCommandHandler(IUnitOfWork uow) : IRequestHandler<SkipQuestionCommand>
{
    public async Task Handle(SkipQuestionCommand request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();

        var result = session.Results.FirstOrDefault(r => r.Id == request.ResultId)
            ?? throw new NotFoundException(nameof(TestResult), request.ResultId);

        // Mark skipped with null isCorrect so it doesn't affect the score or SRS
        result.SubmitAnswer("__skipped__", null, null);

        uow.TestSessions.Update(session);
        await uow.SaveChangesAsync(ct);
    }
}
