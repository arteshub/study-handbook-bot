using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TopicRepository(AppDbContext db) : Repository<Topic>(db), ITopicRepository
{
    public async Task<IReadOnlyList<Topic>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default) =>
        await DbSet.Where(t => t.SubsectionId == subsectionId).ToListAsync(ct);

    public async Task<IReadOnlyList<Topic>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default) =>
        await DbSet
            .Where(t => EF.Property<Guid>(t, "SubsectionId") != Guid.Empty &&
                db.Subsections.Any(sub => sub.SectionId == sectionId && sub.Id == t.SubsectionId))
            .ToListAsync(ct);
}
