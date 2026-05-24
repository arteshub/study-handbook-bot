using HandbookBot.Application.Tests.Dtos;
using HandbookBot.Domain.Enums;
using MediatR;

namespace HandbookBot.Application.Tests.Commands.StartTestSession;

public sealed record StartTestSessionCommand(
    long UserId,
    TestMode Mode,
    Guid? SectionId,
    Guid? SubsectionId,
    Guid? TopicId,
    int QuestionsPerTopic = 3) : IRequest<TestSessionDto>;
