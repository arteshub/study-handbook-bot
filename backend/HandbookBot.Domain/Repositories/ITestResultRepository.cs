using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITestResultRepository : IRepository<TestResult>
{
    Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetTopicsWithWrongAnswersAsync(long userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetWrongCachedQuestionIdsAsync(long userId, CancellationToken ct = default);
}
