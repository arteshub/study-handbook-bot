using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace HandbookBot.Infrastructure.Bot;

internal sealed class TelegramBotService(
    ITelegramBotClient bot,
    IServiceScopeFactory scopeFactory,
    ILogger<TelegramBotService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var options = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery]
        };

        bot.StartReceiving(
            updateHandler: async (_, update, token) =>
            {
                using var scope = scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<BotUpdateHandler>();
                await handler.HandleUpdateAsync(update, token);
            },
            errorHandler: (_, ex, _, _) =>
            {
                logger.LogError(ex, "Telegram polling error");
                return Task.CompletedTask;
            },
            receiverOptions: options,
            cancellationToken: ct);

        await Task.Delay(Timeout.Infinite, ct);
    }
}
