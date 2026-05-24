using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITestResultRepository : IRepository<TestResult>
{
    Task AddRangeAsync(IEnumerable<TestResult> results, CancellationToken ct = default);
}
