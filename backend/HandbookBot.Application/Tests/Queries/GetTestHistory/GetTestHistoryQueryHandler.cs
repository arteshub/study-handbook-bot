using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetTestHistory;

internal sealed class GetTestHistoryQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTestHistoryQuery, IReadOnlyList<TestSessionDto>>
{
    public async Task<IReadOnlyList<TestSessionDto>> Handle(GetTestHistoryQuery request, CancellationToken ct)
    {
        var sessions = await uow.TestSessions.GetByUserIdAsync(request.UserId, ct);

        return sessions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s =>
            {
                int answered = s.Results.Count(r => r.UserAnswer != null || r.IsCorrect.HasValue);
                return new TestSessionDto(s.Id, s.Mode, s.TotalQuestions, s.CorrectAnswers, answered, s.IsCompleted, s.CreatedAt, s.CompletedAt);
            })
            .ToList();
    }
}
