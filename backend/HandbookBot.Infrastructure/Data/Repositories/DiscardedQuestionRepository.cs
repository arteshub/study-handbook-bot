using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class DiscardedQuestionRepository(AppDbContext db) : IDiscardedQuestionRepository
{
    public async Task<IReadOnlySet<Guid>> GetDiscardedIdsAsync(long userId, IReadOnlyList<Guid> cachedQuestionIds, CancellationToken ct = default)
    {
        if (cachedQuestionIds.Count == 0) return new HashSet<Guid>();
        var result = await db.DiscardedQuestions
            .Where(d => d.UserId == userId && cachedQuestionIds.Contains(d.CachedQuestionId))
            .Select(d => d.CachedQuestionId)
            .ToListAsync(ct);
        return result.ToHashSet();
    }

    public Task<bool> ExistsAsync(long userId, Guid cachedQuestionId, CancellationToken ct = default) =>
        db.DiscardedQuestions.AnyAsync(d => d.UserId == userId && d.CachedQuestionId == cachedQuestionId, ct);

    public async Task AddAsync(DiscardedQuestion discard, CancellationToken ct = default) =>
        await db.DiscardedQuestions.AddAsync(discard, ct);
}
