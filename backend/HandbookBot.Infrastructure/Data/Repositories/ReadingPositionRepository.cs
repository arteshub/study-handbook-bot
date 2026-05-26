using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class ReadingPositionRepository(AppDbContext db) : IReadingPositionRepository
{
    public Task<ReadingPosition?> GetAsync(long userId, Guid topicId, CancellationToken ct = default) =>
        db.ReadingPositions.FirstOrDefaultAsync(p => p.UserId == userId && p.TopicId == topicId, ct);

    public async Task AddAsync(ReadingPosition position, CancellationToken ct = default) =>
        await db.ReadingPositions.AddAsync(position, ct);
}
