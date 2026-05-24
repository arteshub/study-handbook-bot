namespace HandbookBot.Application.Common.Interfaces;

public interface IAiService
{
    Task<IReadOnlyList<(string Question, string Answer)>> GenerateQuestionsAsync(
        string topicTitle,
        string topicContent,
        int count,
        string apiKey,
        CancellationToken ct = default);

    Task<(bool IsCorrect, string Feedback)> EvaluateAnswerAsync(
        string question,
        string correctAnswer,
        string userAnswer,
        string apiKey,
        CancellationToken ct = default);
}
