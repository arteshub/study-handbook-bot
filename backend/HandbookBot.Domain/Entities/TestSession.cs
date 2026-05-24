using HandbookBot.Domain.Common;
using HandbookBot.Domain.Enums;
using HandbookBot.Domain.Exceptions;

namespace HandbookBot.Domain.Entities;

public sealed class TestSession : BaseEntity
{
    public long UserId { get; private set; }
    public TestMode Mode { get; private set; }
    public Guid? SectionId { get; private set; }
    public Guid? SubsectionId { get; private set; }
    public Guid? TopicId { get; private set; }
    public int TotalQuestions { get; private set; }
    public int CorrectAnswers { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool IsCompleted => CompletedAt.HasValue;

    private readonly List<TestResult> _results = [];
    public IReadOnlyCollection<TestResult> Results => _results.AsReadOnly();

    private TestSession() { }

    public static TestSession Create(long userId, TestMode mode, Guid? sectionId, Guid? subsectionId, Guid? topicId)
    {
        return new TestSession
        {
            UserId = userId,
            Mode = mode,
            SectionId = sectionId,
            SubsectionId = subsectionId,
            TopicId = topicId
        };
    }

    public void SetTotalQuestions(int total)
    {
        if (total <= 0) throw new DomainException("Total questions must be positive.");
        TotalQuestions = total;
        Touch();
    }

    public void RecordCorrectAnswer()
    {
        CorrectAnswers++;
        Touch();
    }

    public void Complete()
    {
        if (IsCompleted) throw new DomainException("Session is already completed.");
        CompletedAt = DateTime.UtcNow;
        Touch();
    }
}
