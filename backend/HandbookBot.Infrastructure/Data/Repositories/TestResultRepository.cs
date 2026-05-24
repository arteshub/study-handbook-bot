using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TestResultRepository(AppDbContext db) : Repository<TestResult>(db), ITestResultRepository
{
    public async Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default) =>
        await db.TestResults.AddRangeAsync(results, ct);
}
