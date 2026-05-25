using MediatR;

namespace HandbookBot.Application.Tests.Commands.SkipQuestion;

public sealed record SkipQuestionCommand(Guid SessionId, Guid ResultId, long UserId) : IRequest;
