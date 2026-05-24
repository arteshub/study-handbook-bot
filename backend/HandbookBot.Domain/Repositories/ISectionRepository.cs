using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ISectionRepository : IRepository<Section>
{
    Task<IReadOnlyList<Section>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<Section?> GetWithSubsectionsAsync(Guid id, CancellationToken ct = default);
}
