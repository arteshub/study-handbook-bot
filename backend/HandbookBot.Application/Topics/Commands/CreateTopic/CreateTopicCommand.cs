using MediatR;

namespace HandbookBot.Application.Topics.Commands.CreateTopic;

public sealed record CreateTopicCommand(
    Guid SubsectionId,
    long UserId,
    string Title,
    string Content,
    string? Summary,
    Guid? ParentTopicId = null) : IRequest<Guid>;
