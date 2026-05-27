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
        var wrongCount = wrongIds.Count;

        return new ReviewStatsDto(dueCount, 0, wrongCount);
    }
}
