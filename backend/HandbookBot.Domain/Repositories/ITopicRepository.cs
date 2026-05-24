using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITopicRepository : IRepository<Topic>
{
    Task<IReadOnlyList<Topic>> GetBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default);
}
