using HandbookBot.Application.Tests.Dtos;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetTestSession;

public sealed record GetTestSessionQuery(Guid SessionId, long UserId) : IRequest<TestSessionDto>;
