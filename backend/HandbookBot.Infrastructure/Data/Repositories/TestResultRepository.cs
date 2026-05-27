using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TestResultRepository(AppDbContext db) : Repository<TestResult>(db), ITestResultRepository
{
    public async Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default) =>
        await db.TestResults.AddRangeAsync(results, ct);

    public async Task<IReadOnlyList<Guid>> GetTopicsWithWrongAnswersAsync(long userId, CancellationToken ct = default) =>
        await db.TestResults
            .Join(db.TestSessions, r => r.SessionId, s => s.Id, (r, s) => new { r, s })
            .Where(x => x.s.UserId == userId && x.r.IsCorrect == false && x.r.UserAnswer != "__skipped__")
            .Select(x => x.r.TopicId)
            .Distinct()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetWrongCachedQuestionIdsAsync(long userId, CancellationToken ct = default) =>
        await db.TestResults
            .Join(db.TestSessions, r => r.SessionId, s => s.Id, (r, s) => new { r, s })
            .Where(x => x.s.UserId == userId
                && x.r.IsCorrect == false
                && x.r.UserAnswer != "__skipped__"
                && x.r.CachedQuestionId != null)
            .Select(x => x.r.CachedQuestionId!.Value)
            .Distinct()
            .ToListAsync(ct);
}
