using HandbookBot.Application.Interfaces;
using Telegram.Bot;

namespace HandbookBot.Infrastructure.Services;

public class TelegramNotificationService(ITelegramBotClient bot) : INotificationService
{
    public async Task SendAsync(long telegramUserId, string message, CancellationToken ct = default)
    {
        try
        {
            await bot.SendMessage(telegramUserId, message, cancellationToken: ct);
        }
        catch
        {
            // notification failure is non-critical
        }
    }
}
