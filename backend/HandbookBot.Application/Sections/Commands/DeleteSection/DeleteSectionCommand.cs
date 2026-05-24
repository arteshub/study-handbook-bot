using MediatR;

namespace HandbookBot.Application.Sections.Commands.DeleteSection;

public sealed record DeleteSectionCommand(Guid Id, long UserId) : IRequest;
