namespace HandbookBot.Application.Sections.Dtos;

public sealed record SectionDto(
    Guid Id,
    string Title,
    string? Description,
    string Icon,
    int Order,
    int SubsectionCount,
    int TopicCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
