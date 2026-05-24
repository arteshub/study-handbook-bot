using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.SkipTopic;

internal sealed class SkipTopicCommandHandler(IUnitOfWork uow) : IRequestHandler<SkipTopicCommand>
{
    public async Task Handle(SkipTopicCommand request, CancellationToken ct)
    {
        var session = await uow.TestSessions.GetWithResultsAsync(request.SessionId, ct)
            ?? throw new NotFoundException(nameof(TestSession), request.SessionId);

        if (session.UserId != request.UserId) throw new ForbiddenException();

        // Mark all unanswered questions for this topic as skipped (self-marked incorrect)
        var unanswered = session.Results
            .Where(r => r.TopicId == request.TopicId && r.UserAnswer == null && r.IsCorrect == null)
            .ToList();

        foreach (var r in unanswered)
            r.SubmitAnswer("__skipped__", false, null);

        uow.TestSessions.Update(session);
        await uow.SaveChangesAsync(ct);
    }
}
