using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Review.Queries.GetReviewStats;

internal sealed class GetReviewStatsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetReviewStatsQuery, ReviewStatsDto>
{
    public async Task<ReviewStatsDto> Handle(GetReviewStatsQuery request, CancellationToken ct)
    {
        var dueCount = await uow.TopicProgress.GetDueCountAsync(request.UserId, ct);

        var wrongIds = await uow.TestResults.GetWrongCachedQuestionIdsAsync(request.UserId, ct);
        var discardedIds = wrongIds.Count > 0
            ? await uow.DiscardedQuestions.GetDiscardedIdsAsync(request.UserId, wrongIds.ToList(), ct)
            : (IReadOnlySet<Guid>)new HashSet<Guid>();
        var wrongCount = wrongIds.Count(id => !discardedIds.Contains(id));

        return new ReviewStatsDto(dueCount, 0, wrongCount);
    }
}
