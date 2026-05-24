namespace HandbookBot.Application.Subsections.Dtos;

public sealed record SubsectionDto(
    Guid Id,
    Guid SectionId,
    string Title,
    string? Description,
    int Order,
    int TopicCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);
