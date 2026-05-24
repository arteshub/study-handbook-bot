using MediatR;

namespace HandbookBot.Application.Subsections.Commands.CreateSubsection;

public sealed record CreateSubsectionCommand(
    Guid SectionId,
    long UserId,
    string Title,
    string? Description) : IRequest<Guid>;
