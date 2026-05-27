namespace HandbookBot.Application.Interfaces;

public interface IGenerationProgressNotifier
{
    Task NotifyAsync(long userId, Guid topicId, string videoTitle, int progress, bool isCompleted, string? error = null);
}
