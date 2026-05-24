using MediatR;

namespace HandbookBot.Application.Chat;

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatCommand(
    Guid TopicId,
    string QuestionContext,
    string ModelAnswerContext,
    IReadOnlyList<ChatMessage> History,
    string Message) : IRequest<string>;
