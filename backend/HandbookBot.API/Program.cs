using HandbookBot.API.Middleware;
using HandbookBot.Application;
using HandbookBot.Infrastructure;
using HandbookBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

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
app.MapFallbackToFile("index.html");

if (app.Environment.IsProduction())
{
    var bot = app.Services.GetRequiredService<Telegram.Bot.ITelegramBotClient>();
    var webhookUrl = $"{app.Configuration["WebApp:Url"]}/api/telegram/webhook";
    await bot.SetWebhook(webhookUrl);
}

app.Run();
