using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class CachedQuestionRepository(AppDbContext db) : ICachedQuestionRepository
{
    public async Task<IReadOnlyList<CachedQuestion>> GetAsync(Guid topicId, TestMode mode, string contentHash, CancellationToken ct = default) =>
        await db.CachedQuestions
            .Where(q => q.TopicId == topicId && q.Mode == mode && q.ContentHash == contentHash)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<CachedQuestion> questions, CancellationToken ct = default) =>
        await db.CachedQuestions.AddRangeAsync(questions, ct);

    public async Task DeleteStaleAsync(Guid topicId, TestMode mode, string currentContentHash, CancellationToken ct = default)
    {
        var stale = await db.CachedQuestions
            .Where(q => q.TopicId == topicId && q.Mode == mode && q.ContentHash != currentContentHash)
            .ToListAsync(ct);
        if (stale.Count > 0)
            db.CachedQuestions.RemoveRange(stale);
    }
}
