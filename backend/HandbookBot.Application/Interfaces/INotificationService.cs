namespace HandbookBot.Application.Interfaces;

public interface INotificationService
{
    Task SendAsync(long telegramUserId, string message, CancellationToken ct = default);
}
