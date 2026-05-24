namespace HandbookBot.Application.Topics.Dtos;

public sealed record TopicDto(
    Guid Id,
    Guid SubsectionId,
    string Title,
    string Content,
    string? Summary,
    int Order,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TopicListItemDto(
    Guid Id,
    Guid SubsectionId,
    string Title,
    string? Summary,
    int Order,
    DateTime UpdatedAt);
