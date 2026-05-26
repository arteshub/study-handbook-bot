using HandbookBot.API.Hubs;
using HandbookBot.API.Middleware;
using HandbookBot.Application;
using HandbookBot.Infrastructure;
using HandbookBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using YoutubeExplode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddSingleton<YoutubeClient>(_ =>
{
    var http = new HttpClient();
    http.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
    http.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
    // Bypass GDPR consent page (required in EU/Russia server deployments)
    http.DefaultRequestHeaders.Add("Cookie",
        "SOCS=CAESEwgDEgk0OTMzMjMwMjIaAmVuIAEaBgiA_LyaBg; CONSENT=YES+cb");
    return new YoutubeClient(http);
});

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TelegramAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<ReadingHub>("/hubs/reading");
app.MapFallbackToFile("index.html");

if (app.Environment.IsProduction())
{
    var webAppUrl = app.Configuration["WebApp:Url"];
    if (!string.IsNullOrEmpty(webAppUrl))
    {
        var bot = app.Services.GetRequiredService<Telegram.Bot.ITelegramBotClient>();
        await bot.SetWebhook($"{webAppUrl}/api/telegram/webhook");
    }
}

app.Run();
