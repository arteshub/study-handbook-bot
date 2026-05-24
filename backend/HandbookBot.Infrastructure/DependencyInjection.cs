using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Domain.Repositories;
using HandbookBot.Infrastructure.Bot;
using HandbookBot.Infrastructure.Data;
using HandbookBot.Infrastructure.Data.Repositories;
using HandbookBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace HandbookBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(ResolveConnectionString(config)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAiService, OpenAiService>();
        services.AddScoped<BotUpdateHandler>();

        var botToken = config["Telegram:BotToken"]!;
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddHostedService<TelegramBotService>();

        return services;
    }

    private static string ResolveConnectionString(IConfiguration config)
    {
        // Standard .NET connection string
        var cs = config.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(cs)) return cs;

        // Railway / Heroku style: postgresql://user:pass@host:port/db
        var url = config["DATABASE_URL"];
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("No database connection string configured.");

        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}
