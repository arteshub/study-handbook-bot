using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITestSessionRepository : IRepository<TestSession>
{
    Task<IReadOnlyList<TestSession>> GetByUserIdAsync(long userId, CancellationToken ct = default);
    Task<TestSession?> GetWithResultsAsync(Guid id, CancellationToken ct = default);
}
