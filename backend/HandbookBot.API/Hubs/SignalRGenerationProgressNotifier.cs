using HandbookBot.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HandbookBot.API.Hubs;

public class SignalRGenerationProgressNotifier(IHubContext<ReadingHub> hubContext) : IGenerationProgressNotifier
{
    public Task NotifyAsync(long userId, Guid topicId, string videoTitle, int progress, bool isCompleted, string? error = null)
        => hubContext.Clients.Group($"user-{userId}").SendAsync("YoutubeGenerationProgress", new
        {
            topicId,
            videoTitle,
            progress,
            isCompleted,
            error
        });
}
