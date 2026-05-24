namespace HandbookBot.API.Middleware;

internal sealed class TelegramAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var header = ctx.Request.Headers["X-Telegram-User-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(header) && long.TryParse(header, out var userId))
            ctx.Items["TelegramUserId"] = userId;

        await next(ctx);
    }
}
