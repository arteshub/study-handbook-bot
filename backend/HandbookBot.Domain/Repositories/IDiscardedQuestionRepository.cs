using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface IDiscardedQuestionRepository
{
    Task<IReadOnlySet<Guid>> GetDiscardedIdsAsync(long userId, IReadOnlyList<Guid> cachedQuestionIds, CancellationToken ct = default);
    Task<bool> ExistsAsync(long userId, Guid cachedQuestionId, CancellationToken ct = default);
    Task AddAsync(DiscardedQuestion discard, CancellationToken ct = default);
    Task DeleteAsync(long userId, Guid cachedQuestionId, CancellationToken ct = default);
}
