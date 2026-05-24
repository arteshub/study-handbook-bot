using MediatR;

namespace HandbookBot.Application.Subsections.Commands.UpdateSubsection;

public sealed record UpdateSubsectionCommand(
    Guid Id,
    long UserId,
    string Title,
    string? Description) : IRequest;
