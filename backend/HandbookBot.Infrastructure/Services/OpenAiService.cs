using HandbookBot.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace HandbookBot.Infrastructure.Services;

internal sealed class OpenAiService(IConfiguration config) : IAiService
{
    private string ApiKey => config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

    // Self mode gets more content since output is smaller (no options)
    private const int MaxContentCharsSelf = 12000;
    private const int MaxContentCharsAi = 6000;

    // Default SDK network timeout is 100s which is too short for large question batches
    private static readonly OpenAIClientOptions ClientOptions = new() { NetworkTimeout = TimeSpan.FromMinutes(5) };

    private ChatClient CreateClient() => new("gpt-4o-mini", new ApiKeyCredential(ApiKey), ClientOptions);

    public async Task<IReadOnlyList<(string Question, IReadOnlyList<AiOption> Options)>> GenerateQuestionsAsync(
        string topicTitle, string topicContent, int count, CancellationToken ct = default)
    {
        var client = CreateClient();

        var content = topicContent.Length > MaxContentCharsAi
            ? topicContent[..MaxContentCharsAi] + "\n...[обрезано]"
            : topicContent;

        var prompt =
            $"Ты — преподаватель. По теме \"{topicTitle}\" составь ровно {count} вопроса с 4 вариантами ответа.\n" +
            $"Ровно один вариант правильный. Для каждого варианта — КРАТКОЕ объяснение (1 предложение).\n\n" +
            $"Контент темы:\n{content}\n\n" +
            $"Верни ТОЛЬКО JSON-массив без markdown и пояснений:\n" +
            $"[{{\"question\":\"...\",\"options\":[{{\"text\":\"...\",\"isCorrect\":false,\"explanation\":\"...\"}},...]}}]";

        // Russian text costs ~2x more tokens than English; each question has 4 options + explanations (~550 tokens each)
        var outputTokens = Math.Min(1000 + count * 550, 16000);
        var options = new ChatCompletionOptions { MaxOutputTokenCount = outputTokens };
        var response = await client.CompleteChatAsync([new UserChatMessage(prompt)], options, ct);
        var raw = response.Value.Content[0].Text.Trim();
        var json = ExtractJsonArray(raw);

        List<AiQuestionItem> items;
        try
        {
            items = JsonSerializer.Deserialize<List<AiQuestionItem>>(json, JsonOpts) ?? [];
        }
        catch (JsonException)
        {
            // GPT truncated mid-item — keep only fully-formed items
            items = ParsePartialJsonArray(raw);
        }

        return items.Select(i => (
            i.question,
            (IReadOnlyList<AiOption>)i.options.Select(o => new AiOption(o.text, o.isCorrect, o.explanation)).ToList()
        )).ToList();
    }

    public async Task<IReadOnlyList<(string Question, string ModelAnswer)>> GenerateSelfTestQuestionsAsync(
        string topicTitle, string topicContent, int count, CancellationToken ct = default)
    {
        var client = CreateClient();

        var content = topicContent.Length > MaxContentCharsSelf
            ? topicContent[..MaxContentCharsSelf] + "\n...[обрезано]"
            : topicContent;

        var prompt =
            $"Ты — строгий технический интервьюер. По теме \"{topicTitle}\" составь ровно {count} вопроса.\n\n" +
            $"Структура вопросов по порядку:\n" +
            $"1. ПЕРВЫЙ вопрос — всегда базовый: что это такое и как работает в целом. Должен дать общее понимание темы.\n" +
            $"2-N. Остальные вопросы — по нарастающей сложности: механизмы под капотом → конкретные детали и поведение → граничные случаи, ловушки, неочевидное поведение.\n\n" +
            $"Требования:\n" +
            $"- Покрой ВСЕ ключевые концепции из контента\n" +
            $"- Вопросы типа \"что произойдёт если...\", \"чем отличается...\", \"почему...\", \"как именно работает...\"\n" +
            $"- Каждый следующий вопрос сложнее предыдущего\n" +
            $"- Для каждого вопроса — точный эталонный ответ (3-5 предложений) строго из контента\n\n" +
            $"Контент темы:\n{content}\n\n" +
            $"Верни ТОЛЬКО JSON-массив без markdown:\n" +
            $"[{{\"question\":\"...\",\"modelAnswer\":\"...\"}}]";

        // Each Q+A pair is ~200 tokens in Russian
        var outputTokens = Math.Min(300 + count * 200, 16000);
        var options = new ChatCompletionOptions { MaxOutputTokenCount = outputTokens };
        var response = await client.CompleteChatAsync([new UserChatMessage(prompt)], options, ct);
        var json = ExtractJsonArray(response.Value.Content[0].Text.Trim());

        var items = JsonSerializer.Deserialize<List<SelfQaItem>>(json, JsonOpts) ?? [];
        return items.Select(i => (i.question, i.modelAnswer)).ToList();
    }

    public async Task<string> ChatAsync(
        string topicTitle, string topicContent, string questionContext, string modelAnswerContext,
        IReadOnlyList<(string Role, string Content)> history, string userMessage, CancellationToken ct = default)
    {
        var client = CreateClient();

        var content = topicContent.Length > 4000 ? topicContent[..4000] + "\n...[обрезано]" : topicContent;

        var systemPrompt =
            $"Ты — наставник по теме \"{topicTitle}\". Помогаешь глубже разобраться в материале.\n\n" +
            $"Контент темы из справочника:\n{content}\n\n" +
            $"Контекст разговора — вопрос который только что был задан студенту:\n" +
            $"Вопрос: {questionContext}\n" +
            $"Эталонный ответ: {modelAnswerContext}\n\n" +
            $"Отвечай конкретно, по делу. Используй примеры из контента темы. Можешь углублять тему дальше.";

        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
        foreach (var (role, text) in history)
            messages.Add(role == "user" ? new UserChatMessage(text) : new AssistantChatMessage(text));
        messages.Add(new UserChatMessage(userMessage));

        var response = await client.CompleteChatAsync(messages, new ChatCompletionOptions { MaxOutputTokenCount = 1000 }, ct);
        return response.Value.Content[0].Text;
    }

    private static string ExtractJsonArray(string raw)
    {
        if (raw.Contains("```"))
        {
            var fence = raw.IndexOf('\n', raw.IndexOf("```"));
            var end = raw.LastIndexOf("```");
            if (fence >= 0 && end > fence)
                raw = raw[(fence + 1)..end].Trim();
        }

        var start = raw.IndexOf('[');
        var finish = raw.LastIndexOf(']');
        if (start >= 0 && finish > start)
            return raw[start..(finish + 1)];

        return raw;
    }

    // Recovers fully-formed items from a JSON array truncated mid-element
    private static List<AiQuestionItem> ParsePartialJsonArray(string raw)
    {
        var result = new List<AiQuestionItem>();
        var start = raw.IndexOf('[');
        if (start < 0) return result;

        int depth = 0;
        int objStart = -1;
        for (int i = start; i < raw.Length; i++)
        {
            if (raw[i] == '{') { if (depth++ == 0) objStart = i; }
            else if (raw[i] == '}')
            {
                if (--depth == 0 && objStart >= 0)
                {
                    var slice = raw[objStart..(i + 1)];
                    try
                    {
                        var item = JsonSerializer.Deserialize<AiQuestionItem>(slice, JsonOpts);
                        if (item is not null) result.Add(item);
                    }
                    catch { /* skip malformed object */ }
                    objStart = -1;
                }
            }
        }
        return result;
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private record AiOptionItem(string text, bool isCorrect, string explanation);
    private record AiQuestionItem(string question, List<AiOptionItem> options);
    private record SelfQaItem(string question, string modelAnswer);
}
