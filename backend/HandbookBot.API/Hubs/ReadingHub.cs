using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace HandbookBot.API.Hubs;

public sealed class ReadingHub(IUnitOfWork uow) : Hub
{
    public async Task SaveScrollPosition(string topicId, float scrollRatio)
    {
        var userId = GetUserId();
        if (userId is null || !Guid.TryParse(topicId, out var topicGuid)) return;

        var existing = await uow.ReadingPositions.GetAsync(userId.Value, topicGuid);
        if (existing is null)
            await uow.ReadingPositions.AddAsync(ReadingPosition.Create(userId.Value, topicGuid, scrollRatio));
        else
            existing.Update(scrollRatio);

        await uow.SaveChangesAsync();
    }

    private long? GetUserId()
    {
        var httpCtx = Context.GetHttpContext();
        if (httpCtx?.Items.TryGetValue("TelegramUserId", out var v) == true && v is long id)
            return id;
        return null;
    }
}
