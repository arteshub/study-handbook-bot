using HandbookBot.Application.Users.Commands.UpsertUser;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HandbookBot.Infrastructure.Bot;

public sealed class BotUpdateHandler(
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

        var name = System.Web.HttpUtility.HtmlEncode(firstName);
        var text =
            $"👋 Привет, <b>{name}</b>!\n\n" +
            $"Я твой личный справочник знаний — место, где структурированные знания всегда под рукой.\n\n" +
            $"<b>Что умею:</b>\n\n" +
            $"📂 <b>Структура</b>\n" +
            $"Организуй знания по разделам, подразделам и темам — любой глубины вложенности\n\n" +
            $"✍️ <b>Редактор</b>\n" +
            $"Пиши статьи в Markdown: заголовки, списки, таблицы, блоки кода с подсветкой\n\n" +
            $"🎬 <b>Генерация из YouTube</b>\n" +
            $"Вставь ссылку на видео — получи полную структурированную статью по субтитрам. Работает в фоне, не блокирует интерфейс\n\n" +
            $"🧪 <b>Тесты</b>\n" +
            $"Проверяй себя по любому разделу: ИИ генерирует вопросы прямо из твоих материалов. Режим повторения ошибок и отслеживание прогресса\n\n" +
            $"📊 <b>История</b>\n" +
            $"Отслеживай динамику результатов и видь, что уже хорошо усвоено\n\n" +
            $"🔖 <b>Позиция чтения</b>\n" +
            $"Прокрутка сохраняется — открывай любую тему и продолжай с того места, где остановился\n\n" +
            $"<b>Нажми кнопку чтобы открыть</b> 👇";

        await bot.SendMessage(
            chatId,
            text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
}
