namespace HandbookBot.Application.Topics.Dtos;

public sealed record TopicDto(
    Guid Id,
    Guid SubsectionId,
    Guid? ParentTopicId,
    string Title,
    string Content,
    string? Summary,
    int Order,
    int ChildrenCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TopicListItemDto(
    Guid Id,
    Guid SubsectionId,
    Guid? ParentTopicId,
    string Title,
    string? Summary,
    int Order,
    int ChildrenCount,
    DateTime UpdatedAt);

public sealed record TopicTreeNodeDto(
    Guid Id,
    string Title,
    string? Summary,
    bool HasContent,
    int Order,
    IReadOnlyList<TopicTreeNodeDto> Children);

public sealed record SectionTreeDto(
    Guid Id,
    string Title,
    string Icon,
    IReadOnlyList<SubsectionTreeDto> Subsections);

public sealed record SubsectionTreeDto(
    Guid Id,
    string Title,
    IReadOnlyList<TopicTreeNodeDto> Topics);
