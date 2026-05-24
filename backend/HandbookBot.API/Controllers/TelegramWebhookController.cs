using HandbookBot.Infrastructure.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using System.Text.Json;

namespace HandbookBot.API.Controllers;

[ApiController]
[Route("api/telegram")]
public sealed class TelegramWebhookController(BotUpdateHandler handler) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(ct);
        var update = JsonSerializer.Deserialize(json, Telegram.Bot.JsonBotSerializerContext.Default.Update);
        if (update is null) return Ok();
        await handler.HandleUpdateAsync(update, ct);
        return Ok();
    }
}
