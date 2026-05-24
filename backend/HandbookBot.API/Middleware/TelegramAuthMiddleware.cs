using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace HandbookBot.API.Middleware;

internal sealed class TelegramAuthMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var initData = ctx.Request.Headers["X-Telegram-Init-Data"].FirstOrDefault();

        if (!string.IsNullOrEmpty(initData))
        {
            var userId = ValidateInitData(initData);
            if (userId is null)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await ctx.Response.WriteAsync("Invalid Telegram init data.");
                return;
            }

            ctx.Items["TelegramUserId"] = userId.Value;
            await EnsureUserExistsAsync(userId.Value, initData, ctx.RequestAborted);
        }
        else if (env.IsDevelopment())
        {
            // Dev fallback: allow plain user ID header in Development only
            var header = ctx.Request.Headers["X-Telegram-User-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(header) && long.TryParse(header, out var telegramId))
            {
                ctx.Items["TelegramUserId"] = telegramId;
                await EnsureUserExistsAsync(telegramId, null, ctx.RequestAborted);
            }
        }

        await next(ctx);
    }

    private long? ValidateInitData(string initData)
    {
        var botToken = config["Telegram:BotToken"];
        if (string.IsNullOrEmpty(botToken)) return null;

        var parsed = HttpUtility.ParseQueryString(initData);
        var receivedHash = parsed["hash"];
        if (string.IsNullOrEmpty(receivedHash)) return null;

        // Build data-check string: all fields except hash, sorted, key=value joined by \n
        var entries = parsed.AllKeys
            .Where(k => k != "hash")
            .OrderBy(k => k)
            .Select(k => $"{k}={parsed[k]}");
        var dataCheckString = string.Join("\n", entries);

        // secret_key = HMAC-SHA256(key=botToken, data="WebAppData")
        var secretKey = HMACSHA256.HashData(Encoding.UTF8.GetBytes(botToken), Encoding.UTF8.GetBytes("WebAppData"));
        var expectedHash = HMACSHA256.HashData(secretKey, Encoding.UTF8.GetBytes(dataCheckString));
        var expectedHashHex = Convert.ToHexString(expectedHash).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedHashHex),
                Encoding.UTF8.GetBytes(receivedHash.ToLowerInvariant())))
            return null;

        // Extract user.id from the validated "user" field
        var userJson = parsed["user"];
        if (string.IsNullOrEmpty(userJson)) return null;

        using var doc = JsonDocument.Parse(userJson);
        if (doc.RootElement.TryGetProperty("id", out var idEl) && idEl.TryGetInt64(out var userId))
            return userId;

        return null;
    }

    private async Task EnsureUserExistsAsync(long telegramId, string? initData, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = await uow.Users.GetByTelegramIdAsync(telegramId, ct);

        if (user is null)
        {
            // Try to extract name from initData if available
            string firstName = "User";
            string? username = null;
            string? lastName = null;

            if (!string.IsNullOrEmpty(initData))
            {
                var parsed = HttpUtility.ParseQueryString(initData);
                var userJson = parsed["user"];
                if (!string.IsNullOrEmpty(userJson))
                {
                    using var doc = JsonDocument.Parse(userJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("first_name", out var fn)) firstName = fn.GetString() ?? "User";
                    if (root.TryGetProperty("username", out var un)) username = un.GetString();
                    if (root.TryGetProperty("last_name", out var ln)) lastName = ln.GetString();
                }
            }

            user = User.Create(telegramId, firstName, username, lastName);
            await uow.Users.AddAsync(user, ct);
            await uow.SaveChangesAsync(ct);
        }
    }
}
