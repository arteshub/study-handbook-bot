using HandbookBot.Application.Topics.Dtos;
using MediatR;

namespace HandbookBot.Application.Topics.Queries.GetTopics;

public sealed record GetTopicsQuery(Guid SubsectionId, long UserId) : IRequest<IReadOnlyList<TopicListItemDto>>;
