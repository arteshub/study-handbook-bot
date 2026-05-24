using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Domain.Repositories;
using HandbookBot.Infrastructure.Bot;
using HandbookBot.Infrastructure.Data;
using HandbookBot.Infrastructure.Data.Repositories;
using HandbookBot.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace HandbookBot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(ResolveConnectionString(config)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAiService, OpenAiService>();
        services.AddScoped<BotUpdateHandler>();

        var botToken = config["Telegram:BotToken"]!;
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddScoped<BotUpdateHandler>();

        // Polling only in Development; Production uses webhook
        if (env.IsDevelopment())
            services.AddHostedService<TelegramBotService>();

        return services;
    }

    private static string ResolveConnectionString(IConfiguration config)
    {
        // Railway/Heroku DATABASE_URL takes priority over appsettings (which may contain docker-compose hostname)
        var url = config["DATABASE_URL"];
        if (!string.IsNullOrEmpty(url))
        {
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');
            return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        }

        var cs = config.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(cs)) return cs;

        throw new InvalidOperationException("No database connection string configured. Set DATABASE_URL or ConnectionStrings__DefaultConnection.");
    }
}
