using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class SectionRepository(AppDbContext db) : Repository<Section>(db), ISectionRepository
{
    public async Task<IReadOnlyList<Section>> GetByUserIdAsync(long userId, CancellationToken ct = default) =>
        await DbSet
            .Include(s => s.Subsections).ThenInclude(sub => sub.Topics)
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

    public Task<Section?> GetWithSubsectionsAsync(Guid id, CancellationToken ct = default) =>
        DbSet
            .Include(s => s.Subsections).ThenInclude(sub => sub.Topics)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
}
