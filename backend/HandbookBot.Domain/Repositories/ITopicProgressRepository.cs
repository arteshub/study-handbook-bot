using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface ITopicProgressRepository : IRepository<TopicProgress>
{
    Task<TopicProgress?> GetByUserAndTopicAsync(long userId, Guid topicId, CancellationToken ct = default);
    Task<IReadOnlyList<TopicProgress>> GetDueForReviewAsync(long userId, CancellationToken ct = default);
    Task<int> GetDueCountAsync(long userId, CancellationToken ct = default);
}
