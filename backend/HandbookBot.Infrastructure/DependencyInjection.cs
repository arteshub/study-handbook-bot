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
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAiService, OpenAiService>();
        services.AddScoped<BotUpdateHandler>();

        var botToken = config["Telegram:BotToken"]!;
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
        services.AddHostedService<TelegramBotService>();

        return services;
    }
}
