namespace HandbookBot.Domain.Repositories;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ISectionRepository Sections { get; }
    ISubsectionRepository Subsections { get; }
    ITopicRepository Topics { get; }
    ITestSessionRepository TestSessions { get; }
    ITestResultRepository TestResults { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
