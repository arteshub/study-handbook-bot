using MediatR;

namespace HandbookBot.Application.Topics.Commands.DeleteTopicLink;

public sealed record DeleteTopicLinkCommand(Guid TopicId, Guid LinkId, long UserId) : IRequest;
