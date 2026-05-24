using HandbookBot.Infrastructure.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace HandbookBot.API.Controllers;

[ApiController]
[Route("api/telegram")]
public sealed class TelegramWebhookController(BotUpdateHandler handler) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken ct)
    {
        await handler.HandleUpdateAsync(update, ct);
        return Ok();
    }
}
