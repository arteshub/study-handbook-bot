using HandbookBot.Domain.Common;
using HandbookBot.Domain.Enums;

namespace HandbookBot.Domain.Entities;

public sealed class CachedQuestion : BaseEntity
{
    public Guid TopicId { get; private set; }
    public TestMode Mode { get; private set; }
    public string ContentHash { get; private set; } = string.Empty;
    public string QuestionText { get; private set; } = string.Empty;
    public string? ModelAnswer { get; private set; }
    public string? OptionsJson { get; private set; }

    private CachedQuestion() { }

    public static CachedQuestion CreateAi(Guid topicId, string contentHash, string questionText, string optionsJson) =>
        new() { TopicId = topicId, Mode = TestMode.AI, ContentHash = contentHash, QuestionText = questionText, OptionsJson = optionsJson };

    public static CachedQuestion CreateSelf(Guid topicId, string contentHash, string questionText, string modelAnswer) =>
        new() { TopicId = topicId, Mode = TestMode.Self, ContentHash = contentHash, QuestionText = questionText, ModelAnswer = modelAnswer };
}
