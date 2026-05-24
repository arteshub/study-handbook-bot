using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class TopicRepository(AppDbContext db) : Repository<Topic>(db), ITopicRepository
{
    public async Task<IReadOnlyList<Topic>> GetRootsBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default) =>
        await DbSet
            .Where(t => t.SubsectionId == subsectionId && t.ParentTopicId == null)
            .OrderBy(t => t.Order).ThenBy(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Topic>> GetChildrenAsync(Guid parentTopicId, CancellationToken ct = default) =>
        await DbSet
            .Where(t => t.ParentTopicId == parentTopicId)
            .OrderBy(t => t.Order).ThenBy(t => t.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Topic>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default) =>
        await DbSet
            .Where(t => db.Subsections.Any(sub => sub.SectionId == sectionId && sub.Id == t.SubsectionId))
            .ToListAsync(ct);

    public Task<Topic?> GetWithChildrenAsync(Guid id, CancellationToken ct = default) =>
        DbSet.Include(t => t.Children).FirstOrDefaultAsync(t => t.Id == id, ct);
}
