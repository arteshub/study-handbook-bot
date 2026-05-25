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

        var wrongTopicIds = await uow.TestResults.GetTopicsWithWrongAnswersAsync(request.UserId, ct);
        var wrongCount = await uow.CachedQuestions.CountByTopicsAsync(wrongTopicIds, TestMode.Self, ct);

        return new ReviewStatsDto(dueCount, 0, wrongCount);
    }
}
