using HandbookBot.Application.Topics.Dtos;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.AddTopicLink;

public sealed record AddTopicLinkCommand(Guid TopicId, long UserId, string Title, string Url)
    : IRequest<TopicLinkDto>;
