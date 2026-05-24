using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITopicRepository : IRepository<Topic>
{
    Task<IReadOnlyList<Topic>> GetRootsBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> GetAllBySubsectionIdAsync(Guid subsectionId, CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> GetChildrenAsync(Guid parentTopicId, CancellationToken ct = default);
    Task<IReadOnlyList<Topic>> GetBySectionIdAsync(Guid sectionId, CancellationToken ct = default);
    Task<Topic?> GetWithChildrenAsync(Guid id, CancellationToken ct = default);
}
