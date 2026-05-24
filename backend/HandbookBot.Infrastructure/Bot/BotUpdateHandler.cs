using HandbookBot.Application.Users.Commands.UpsertUser;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HandbookBot.Infrastructure.Bot;

internal sealed class BotUpdateHandler(
    ITelegramBotClient bot,
    IMediator mediator,
    IConfiguration config,
    ILogger<BotUpdateHandler> logger)
{
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        if (update.Message is { } message)
            await HandleMessageAsync(message, ct);
    }

    private async Task HandleMessageAsync(Message message, CancellationToken ct)
    {
        if (message.From is null) return;

        await mediator.Send(new UpsertUserCommand(
            message.From.Id,
            message.From.FirstName,
            message.From.Username,
            message.From.LastName), ct);

        if (message.Text?.StartsWith("/start") == true)
            await SendWelcomeAsync(message.Chat.Id, message.From.FirstName, ct);
    }

    private async Task SendWelcomeAsync(long chatId, string firstName, CancellationToken ct)
    {
        var webAppUrl = config["WebApp:Url"]!;
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithWebApp("📚 Открыть справочник", new WebAppInfo { Url = webAppUrl }) }
        });

        await bot.SendMessage(
            chatId,
            $"👋 Привет, *{firstName}*\\!\n\nЯ твой личный справочник знаний\\.\nСоздавай разделы, добавляй темы и проверяй себя через тесты\\!\n\n*Нажми кнопку* чтобы открыть интерфейс 👇",
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
}
