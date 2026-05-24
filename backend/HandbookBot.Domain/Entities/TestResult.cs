using HandbookBot.Domain.Common;

namespace HandbookBot.Domain.Entities;

public sealed class TestResult : BaseEntity
{
    public Guid SessionId { get; private set; }
    public Guid TopicId { get; private set; }
    public string Question { get; private set; } = string.Empty;
    public string CorrectAnswer { get; private set; } = string.Empty;
    public string? OptionsJson { get; private set; }
    public string? UserAnswer { get; private set; }
    public bool? IsCorrect { get; private set; }
    public string? AiFeedback { get; private set; }
    public int Order { get; private set; }

    private TestResult() { }

    public static TestResult Create(Guid sessionId, Guid topicId, string question, string correctAnswer, int order, string? optionsJson = null)
    {
        return new TestResult
        {
            SessionId = sessionId,
            TopicId = topicId,
            Question = question,
            CorrectAnswer = correctAnswer,
            OptionsJson = optionsJson,
            Order = order
        };
    }

    public void SubmitAnswer(string? userAnswer, bool? isCorrect, string? aiFeedback)
    {
        UserAnswer = userAnswer;
        IsCorrect = isCorrect;
        AiFeedback = aiFeedback;
        Touch();
    }
}
