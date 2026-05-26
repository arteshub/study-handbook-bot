using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface IReadingPositionRepository
{
    Task<ReadingPosition?> GetAsync(long userId, Guid topicId, CancellationToken ct = default);
    Task AddAsync(ReadingPosition position, CancellationToken ct = default);
}
