using MediatR;

namespace HandbookBot.Application.Tests.Commands.SkipTopic;

public sealed record SkipTopicCommand(Guid SessionId, Guid TopicId, long UserId) : IRequest;
