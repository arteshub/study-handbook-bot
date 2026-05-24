using HandbookBot.Application.Tests.Dtos;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.CompleteTestSession;

public sealed record CompleteTestSessionCommand(Guid SessionId, long UserId) : IRequest<TestSessionDto>;
