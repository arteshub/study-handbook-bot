using MediatR;

namespace HandbookBot.Application.Sections.Commands.UpdateSection;

public sealed record UpdateSectionCommand(
    Guid Id,
    long UserId,
    string Title,
    string? Description,
    string? Icon) : IRequest;
