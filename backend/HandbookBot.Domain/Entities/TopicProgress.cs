using HandbookBot.Domain.Common;
using HandbookBot.Domain.Enums;

namespace HandbookBot.Domain.Entities;

public sealed class TopicProgress : BaseEntity
{
    public long UserId { get; private set; }
    public Guid TopicId { get; private set; }
    public int CorrectStreak { get; private set; }
    public float EaseFactor { get; private set; }
    public int IntervalDays { get; private set; }
    public DateTime NextReviewAt { get; private set; }
    public MasteryLevel Mastery { get; private set; }

    private TopicProgress() { }

    public static TopicProgress Create(long userId, Guid topicId) => new()
    {
        UserId = userId,
        TopicId = topicId,
        CorrectStreak = 0,
        EaseFactor = 2.5f,
        IntervalDays = 1,
        NextReviewAt = DateTime.UtcNow,
        Mastery = MasteryLevel.New,
    };

    private const float MaxEaseFactor = 2.5f;
    private const int MaxIntervalDays = 365;

    // SM-2 algorithm
    public void RecordReview(bool wasCorrect)
    {
        if (wasCorrect)
        {
            CorrectStreak++;
            IntervalDays = CorrectStreak switch
            {
                1 => 1,
                2 => 6,
                _ => Math.Min(MaxIntervalDays, (int)Math.Round((double)IntervalDays * EaseFactor)),
            };
            EaseFactor = Math.Min(MaxEaseFactor, Math.Max(1.3f, EaseFactor + 0.1f));
        }
        else
        {
            CorrectStreak = 0;
            IntervalDays = 1;
            EaseFactor = Math.Max(1.3f, EaseFactor - 0.2f);
        }

        NextReviewAt = DateTime.UtcNow.AddDays(IntervalDays);
        Mastery = IntervalDays >= 21 ? MasteryLevel.Mature
            : IntervalDays >= 7 ? MasteryLevel.Review
            : CorrectStreak > 0 ? MasteryLevel.Learning
            : MasteryLevel.New;

        Touch();
    }
}
