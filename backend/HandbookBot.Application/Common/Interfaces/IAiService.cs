namespace HandbookBot.Application.Common.Interfaces;

public sealed record AiOption(string Text, bool IsCorrect, string Explanation);

public interface IAiService
{
    Task<IReadOnlyList<(string Question, IReadOnlyList<AiOption> Options)>> GenerateQuestionsAsync(
        string topicTitle,
        string topicContent,
        int count,
        CancellationToken ct = default);

    Task<IReadOnlyList<(string Question, string ModelAnswer)>> GenerateSelfTestQuestionsAsync(
        string topicTitle,
        string topicContent,
        int count,
        CancellationToken ct = default);

    Task<string> ChatAsync(
        string topicTitle,
        string topicContent,
        string questionContext,
        string modelAnswerContext,
        IReadOnlyList<(string Role, string Content)> history,
        string userMessage,
        CancellationToken ct = default);
}
