using HandbookBot.Application.Topics.Dtos;
using MediatR;

namespace HandbookBot.Application.Topics.Queries.GetTopicById;

public sealed record GetTopicByIdQuery(Guid Id, long UserId) : IRequest<TopicDto>;
