using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TopicProgressRepository(AppDbContext db) : Repository<TopicProgress>(db), ITopicProgressRepository
{
    public Task<TopicProgress?> GetByUserAndTopicAsync(long userId, Guid topicId, CancellationToken ct = default) =>
        DbSet.FirstOrDefaultAsync(p => p.UserId == userId && p.TopicId == topicId, ct);

    public async Task<IReadOnlyList<TopicProgress>> GetDueForReviewAsync(long userId, CancellationToken ct = default) =>
        await DbSet
            .Where(p => p.UserId == userId && p.NextReviewAt <= DateTime.UtcNow)
            .OrderBy(p => p.NextReviewAt)
            .ToListAsync(ct);

    public Task<int> GetDueCountAsync(long userId, CancellationToken ct = default) =>
        DbSet.CountAsync(p => p.UserId == userId && p.NextReviewAt <= DateTime.UtcNow, ct);
}
