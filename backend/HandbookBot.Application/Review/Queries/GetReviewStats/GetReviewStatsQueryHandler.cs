using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Review.Queries.GetReviewStats;

internal sealed class GetReviewStatsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetReviewStatsQuery, ReviewStatsDto>
{
    public async Task<ReviewStatsDto> Handle(GetReviewStatsQuery request, CancellationToken ct)
    {
        var dueCount = await uow.TopicProgress.GetDueCountAsync(request.UserId, ct);
        return new ReviewStatsDto(dueCount, 0);
    }
}
