using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class SubsectionRepository(AppDbContext db) : Repository<Subsection>(db), ISubsectionRepository
{
    public async Task<IReadOnlyList<Subsection>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default) =>
        await DbSet
            .Include(s => s.Topics)
            .Where(s => s.SectionId == sectionId)
            .ToListAsync(ct);

    public Task<Subsection?> GetWithTopicsAsync(Guid id, CancellationToken ct = default) =>
        DbSet.Include(s => s.Topics).FirstOrDefaultAsync(s => s.Id == id, ct);
}
