using HandbookBot.Domain.Common;

namespace HandbookBot.Domain.Entities;

public sealed class DiscardedQuestion : BaseEntity
{
    public long UserId { get; private set; }
    public Guid CachedQuestionId { get; private set; }

    private DiscardedQuestion() { }

    public static DiscardedQuestion Create(long userId, Guid cachedQuestionId) =>
        new() { UserId = userId, CachedQuestionId = cachedQuestionId };
}
