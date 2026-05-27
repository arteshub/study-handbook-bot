using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;

namespace HandbookBot.Domain.Repositories;

public interface ICachedQuestionRepository
{
    Task<IReadOnlyList<CachedQuestion>> GetAsync(Guid topicId, TestMode mode, string contentHash, CancellationToken ct = default);
    Task<IReadOnlyList<CachedQuestion>> GetAllByTopicAsync(Guid topicId, TestMode mode, CancellationToken ct = default);
    Task<int> CountByTopicsAsync(IReadOnlyList<Guid> topicIds, TestMode mode, CancellationToken ct = default);
    Task<CachedQuestion?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CachedQuestion>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<CachedQuestion> questions, CancellationToken ct = default);
    Task DeleteByIdAsync(Guid id, CancellationToken ct = default);
    Task DeleteStaleAsync(Guid topicId, TestMode mode, string currentContentHash, CancellationToken ct = default);
}
