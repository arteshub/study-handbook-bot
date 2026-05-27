using System.Collections.Concurrent;
using HandbookBot.Application.Interfaces;
using HandbookBot.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HandbookBot.Infrastructure.Services;

public class BackgroundGenerationService(IServiceScopeFactory scopeFactory) : IBackgroundGenerationService
{
    private record GenerationJob(Guid TopicId, long UserId, string VideoTitle, int Progress, bool IsCompleted, string? Error);

    private readonly ConcurrentDictionary<Guid, GenerationJob> _jobs = new();
    private readonly ConcurrentDictionary<long, int> _userActiveCount = new();
    private const int MaxPerUser = 2;

    public bool TryStartGeneration(long userId, Guid topicId, string url, string videoTitle)
    {
        var acquired = false;
        _userActiveCount.AddOrUpdate(
            userId,
            _ => { acquired = true; return 1; },
            (_, c) =>
            {
                if (c < MaxPerUser) { acquired = true; return c + 1; }
                return c;
            });

        if (!acquired) return false;

        _jobs[topicId] = new GenerationJob(topicId, userId, videoTitle, 0, false, null);
        _ = RunGenerationAsync(userId, topicId, url, videoTitle);
        return true;
    }

    public IReadOnlyList<ActiveGenerationDto> GetActiveJobs(long userId)
        => _jobs.Values
            .Where(j => j.UserId == userId && !j.IsCompleted)
            .Select(j => new ActiveGenerationDto(j.TopicId, j.VideoTitle, j.Progress))
            .ToList();

    private async Task RunGenerationAsync(long userId, Guid topicId, string url, string videoTitle)
    {
        using var scope = scopeFactory.CreateScope();
        var youtubeService = scope.ServiceProvider.GetRequiredService<IYoutubeExtractionService>();
        var notifier = scope.ServiceProvider.GetRequiredService<IGenerationProgressNotifier>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var progress = new Progress<int>(p =>
        {
            if (_jobs.TryGetValue(topicId, out var job))
                _jobs[topicId] = job with { Progress = p };
            _ = notifier.NotifyAsync(userId, topicId, videoTitle, p, false);
        });

        try
        {
            var result = await youtubeService.ExtractAndGenerateAsync(url, progress);

            var topic = await uow.Topics.GetByIdAsync(topicId);
            if (topic is not null)
            {
                topic.Update(result.Title, result.Content, topic.Summary);
                uow.Topics.Update(topic);
                await uow.SaveChangesAsync();
            }

            if (_jobs.TryGetValue(topicId, out var completedJob))
                _jobs[topicId] = completedJob with { Progress = 100, IsCompleted = true, VideoTitle = result.Title };

            await notifier.NotifyAsync(userId, topicId, result.Title, 100, true);
            await notificationService.SendAsync(userId, $"✅ Статья \"{result.Title}\" готова!");
        }
        catch (Exception ex)
        {
            if (_jobs.TryGetValue(topicId, out var failedJob))
                _jobs[topicId] = failedJob with { IsCompleted = true, Error = ex.Message };

            await notifier.NotifyAsync(userId, topicId, videoTitle, 0, true, ex.Message);
            await notificationService.SendAsync(userId, $"❌ Ошибка генерации \"{videoTitle}\": {ex.Message}");
        }
        finally
        {
            _userActiveCount.AddOrUpdate(userId, 0, (_, c) => Math.Max(0, c - 1));
        }
    }
}
