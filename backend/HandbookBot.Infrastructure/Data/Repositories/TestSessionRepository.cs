using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TestSessionRepository(AppDbContext db) : Repository<TestSession>(db), ITestSessionRepository
{
    public async Task<IReadOnlyList<TestSession>> GetByUserIdAsync(long userId, CancellationToken ct = default) =>
        await DbSet.Include(s => s.Results).Where(s => s.UserId == userId).ToListAsync(ct);

    public Task<TestSession?> GetWithResultsAsync(Guid id, CancellationToken ct = default) =>
        DbSet.Include(s => s.Results).FirstOrDefaultAsync(s => s.Id == id, ct);
}
