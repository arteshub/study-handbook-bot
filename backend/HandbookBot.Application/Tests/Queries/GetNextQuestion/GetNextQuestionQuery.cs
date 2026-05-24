using HandbookBot.Application.Tests.Dtos;
using MediatR;

namespace HandbookBot.Application.Tests.Queries.GetNextQuestion;

public sealed record GetNextQuestionQuery(Guid SessionId, long UserId) : IRequest<TestQuestionDto?>;
