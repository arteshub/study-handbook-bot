using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;

namespace HandbookBot.Domain.Repositories;

public interface ICachedQuestionRepository
{
    Task<IReadOnlyList<CachedQuestion>> GetAsync(Guid topicId, TestMode mode, string contentHash, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<CachedQuestion> questions, CancellationToken ct = default);
    Task DeleteStaleAsync(Guid topicId, TestMode mode, string currentContentHash, CancellationToken ct = default);
}
