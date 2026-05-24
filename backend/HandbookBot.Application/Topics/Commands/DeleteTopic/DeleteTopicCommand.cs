using MediatR;

namespace HandbookBot.Application.Topics.Commands.DeleteTopic;

public sealed record DeleteTopicCommand(Guid Id, long UserId) : IRequest;
