using HandbookBot.Application.Tests.Dtos;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetTestHistory;

public sealed record GetTestHistoryQuery(long UserId) : IRequest<IReadOnlyList<TestSessionDto>>;
