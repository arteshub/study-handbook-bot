using HandbookBot.Application.Common.Interfaces;
using OpenAI.Chat;

namespace HandbookBot.Infrastructure.Services;

internal sealed class OpenAiService : IAiService
{
    public async Task<IReadOnlyList<(string Question, string Answer)>> GenerateQuestionsAsync(
        string topicTitle, string topicContent, int count, string apiKey, CancellationToken ct = default)
    {
        var client = new ChatClient("gpt-4o-mini", apiKey);

        var prompt = string.Format(
            "Ты — преподаватель. По теме \"{0}\" составь ровно {1} вопроса для проверки знаний.\n" +
            "Контент темы:\n{2}\n\n" +
            "Верни ТОЛЬКО JSON-массив объектов: [{\"question\":\"...\",\"answer\":\"...\"}]\n" +
            "Ответы должны быть краткими и точными.",
            topicTitle, count, topicContent);

        var response = await client.CompleteChatAsync([new UserChatMessage(prompt)], cancellationToken: ct);
        var json = CleanJson(response.Value.Content[0].Text.Trim());

        var items = System.Text.Json.JsonSerializer.Deserialize<List<QaItem>>(json) ?? [];
        return items.Select(i => (i.question, i.answer)).ToList();
    }

    public async Task<(bool IsCorrect, string Feedback)> EvaluateAnswerAsync(
        string question, string correctAnswer, string userAnswer, string apiKey, CancellationToken ct = default)
    {
        var client = new ChatClient("gpt-4o-mini", apiKey);

        var prompt = string.Format(
            "Вопрос: {0}\nПравильный ответ: {1}\nОтвет пользователя: {2}\n\n" +
            "Оцени ответ. Верни JSON: {\"is_correct\": true/false, \"feedback\": \"краткий комментарий\"}",
            question, correctAnswer, userAnswer);

        var response = await client.CompleteChatAsync([new UserChatMessage(prompt)], cancellationToken: ct);
        var json = CleanJson(response.Value.Content[0].Text.Trim());

        var result = System.Text.Json.JsonSerializer.Deserialize<EvalResult>(json);
        return (result?.is_correct ?? false, result?.feedback ?? string.Empty);
    }

    private static string CleanJson(string raw) =>
        raw.StartsWith("```") ? string.Join('\n', raw.Split('\n')[1..^1]) : raw;

    private record QaItem(string question, string answer);
    private record EvalResult(bool is_correct, string feedback);
}
