using MediatR;

namespace HandbookBot.Application.Topics.Commands.UpdateTopic;

public sealed record UpdateTopicCommand(
    Guid Id,
    long UserId,
    string Title,
    string Content,
    string? Summary) : IRequest;
