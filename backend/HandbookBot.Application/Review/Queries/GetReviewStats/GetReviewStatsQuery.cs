using MediatR;

namespace HandbookBot.Application.Review.Queries.GetReviewStats;

public sealed record GetReviewStatsQuery(long UserId) : IRequest<ReviewStatsDto>;

public sealed record ReviewStatsDto(int DueCount, int TotalTracked);
