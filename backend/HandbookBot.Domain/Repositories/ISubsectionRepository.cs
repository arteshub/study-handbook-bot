using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ISubsectionRepository : IRepository<Subsection>
{
    Task<IReadOnlyList<Subsection>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default);
    Task<Subsection?> GetWithTopicsAsync(Guid id, CancellationToken ct = default);
}
