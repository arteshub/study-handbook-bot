using MediatR;

namespace HandbookBot.Application.Users.Commands.UpsertUser;

public sealed record UpsertUserCommand(
    long TelegramId,
    string FirstName,
    string? Username,
    string? LastName) : IRequest;
