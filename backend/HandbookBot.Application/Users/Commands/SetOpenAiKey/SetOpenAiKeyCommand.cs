using MediatR;

namespace HandbookBot.Application.Users.Commands.SetOpenAiKey;

public sealed record SetOpenAiKeyCommand(long TelegramId, string ApiKey) : IRequest;
