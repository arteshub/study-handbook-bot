using MediatR;

namespace HandbookBot.Application.Subsections.Commands.DeleteSubsection;

public sealed record DeleteSubsectionCommand(Guid Id, long UserId) : IRequest;
