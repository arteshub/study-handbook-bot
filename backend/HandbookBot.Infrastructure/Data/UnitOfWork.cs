using HandbookBot.Domain.Repositories;
using HandbookBot.Infrastructure.Data.Repositories;

namespace HandbookBot.Infrastructure.Data;

internal sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(db);
    public ISectionRepository Sections { get; } = new SectionRepository(db);
    public ISubsectionRepository Subsections { get; } = new SubsectionRepository(db);
    public ITopicRepository Topics { get; } = new TopicRepository(db);
    public ITestSessionRepository TestSessions { get; } = new TestSessionRepository(db);
    public ITestResultRepository TestResults { get; } = new TestResultRepository(db);
    public ITopicProgressRepository TopicProgress { get; } = new TopicProgressRepository(db);
    public ICachedQuestionRepository CachedQuestions { get; } = new CachedQuestionRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
