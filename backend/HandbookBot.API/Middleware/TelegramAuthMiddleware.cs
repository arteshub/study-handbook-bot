using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;

namespace HandbookBot.API.Middleware;

internal sealed class TelegramAuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var header = ctx.Request.Headers["X-Telegram-User-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(header) && long.TryParse(header, out var telegramId))
        {
            ctx.Items["TelegramUserId"] = telegramId;
            await EnsureUserExistsAsync(telegramId, ctx.RequestAborted);
        }

        await next(ctx);
    }

    private async Task EnsureUserExistsAsync(long telegramId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await uow.Users.GetByTelegramIdAsync(telegramId, ct);
        if (user is not null) return;

        user = User.Create(telegramId, "User", null, null);
        await uow.Users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);
    }
}
