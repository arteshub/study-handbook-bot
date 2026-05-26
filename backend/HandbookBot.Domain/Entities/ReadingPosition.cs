using HandbookBot.Domain.Common;

namespace HandbookBot.Domain.Entities;

public sealed class ReadingPosition : BaseEntity
{
    public long UserId { get; private set; }
    public Guid TopicId { get; private set; }
    public float ScrollRatio { get; private set; }

    private ReadingPosition() { }

    public static ReadingPosition Create(long userId, Guid topicId, float scrollRatio) =>
        new() { UserId = userId, TopicId = topicId, ScrollRatio = scrollRatio };

    public void Update(float scrollRatio)
    {
        ScrollRatio = Math.Clamp(scrollRatio, 0f, 1f);
        Touch();
    }
}
