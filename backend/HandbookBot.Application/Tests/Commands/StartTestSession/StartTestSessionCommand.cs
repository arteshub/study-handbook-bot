using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Enums;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.StartTestSession;

public sealed record StartTestSessionCommand(
    long UserId,
    TestMode Mode,
    IReadOnlyList<Guid>? SectionIds,
    IReadOnlyList<Guid>? SubsectionIds,
    IReadOnlyList<Guid>? TopicIds,
    bool ReviewMode = false,
    int QuestionsPerTopic = 3) : IRequest<TestSessionDto>;
