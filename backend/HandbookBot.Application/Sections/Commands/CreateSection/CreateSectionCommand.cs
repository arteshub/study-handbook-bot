using MediatR;

namespace HandbookBot.Application.Sections.Commands.CreateSection;

public sealed record CreateSectionCommand(
    long UserId,
    string Title,
    string? Description,
    string? Icon) : IRequest<Guid>;
