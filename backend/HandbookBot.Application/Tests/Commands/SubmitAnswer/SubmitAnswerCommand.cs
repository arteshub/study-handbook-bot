using HandbookBot.Application.Tests.Dtos;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.SubmitAnswer;

public sealed record SubmitAnswerCommand(
    Guid SessionId,
    Guid ResultId,
    long UserId,
    string? UserAnswer,
    bool? SelfMarkedCorrect,
    int? SelectedOptionIndex) : IRequest<TestAnswerResultDto>;
