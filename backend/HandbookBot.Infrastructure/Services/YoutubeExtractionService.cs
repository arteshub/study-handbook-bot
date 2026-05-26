using System.ClientModel;
using System.Text;
using HandbookBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using YoutubeExplode;
using YoutubeExplode.Exceptions;

namespace HandbookBot.Infrastructure.Services;

public class YoutubeExtractionService(YoutubeClient youtubeClient, IConfiguration config) : IYoutubeExtractionService
{
    private string ApiKey => config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

    public async Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, CancellationToken ct = default)
    {
        try
        {
            return await ExtractInternalAsync(url, ct);
        }
        catch (VideoUnavailableException ex)
        {
            throw new InvalidOperationException($"Видео недоступно с сервера: {ex.Message}", ex);
        }
        catch (YoutubeExplodeException ex)
        {
            throw new InvalidOperationException($"Ошибка YouTube: {ex.Message}", ex);
        }
    }

    private async Task<YoutubeExtractResult> ExtractInternalAsync(string url, CancellationToken ct)
    {
        var video = await youtubeClient.Videos.GetAsync(url, ct);
        var videoTitle = video.Title;

        var manifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(url, ct);
        var trackInfo = manifest.Tracks
            .FirstOrDefault(t => t.Language.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            ?? manifest.Tracks.FirstOrDefault()
            ?? throw new InvalidOperationException("Видео не содержит субтитров.");

        var track = await youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo, ct);

        var sb = new StringBuilder();
        foreach (var caption in track.Captions)
        {
            var offset = caption.Offset;
            sb.AppendLine($"[{(int)offset.TotalMinutes:D2}:{offset.Seconds:D2}] {caption.Text}");
        }
        var subtitles = sb.ToString();

        const string styleExample =
            "## ЧАСТЬ I. МАССИВЫ\n\n" +
            "### 1. Что такое массив [00:42 – 01:46]\n\n" +
            "Массив — структура данных фиксированного размера, которая хранит элементы последовательно в памяти (это критично важно — именно последовательность даёт O(1) доступ по индексу). Все элементы одного типа.\n\n" +
            "У каждого элемента есть:\n\n" +
            "- адрес — место в памяти\n" +
            "- размер — сколько байт занимает\n\n" +
            "Индексация начинается с нуля. Пример: если элементы по 2 байта и массив из 4 элементов — то элементы располагаются последовательно один за другим. Слева и справа от массива в памяти — чужие данные, не принадлежащие массиву.\n\n" +
            "### 2. Создание массива — все варианты синтаксиса [01:46 – 02:49]\n\n" +
            "```go\n" +
            "// 1) Просто объявление - zero value у int это 0\n" +
            "var a [5]int\n\n" +
            "// 2) С инициализацией части - остальные элементы zero value\n" +
            "b := [5]int{1, 2, 3}  // [1 2 3 0 0]\n\n" +
            "// 3) Три точки - компилятор сам считает количество элементов\n" +
            "c := [...]int{1, 2, 3}  // тип [3]int\n" +
            "```\n\n" +
            "Ключевой принцип: массив всегда инициализируется zero value соответствующего типа. У int это 0.\n\n" +
            "### 3. Массив — это НЕ указатель [05:36 – 06:18]\n\n" +
            "Многие говорят: «массив указывает на какую-то область памяти». Это неверно. Внутри массива нет никаких указателей. Массив — это просто структура данных, у которой есть адрес начала, длина (зашита в типе) и размер элемента.\n\n" +
            "## ШПАРГАЛКА ДЛЯ СОБЕСА\n\n" +
            "| | Массив | Слайс |\n" +
            "|---|---|---|\n" +
            "| Размер | Часть типа, фиксированный | Динамический |\n" +
            "| Zero value | Все элементы zero (НЕ nil) | nil |\n" +
            "| Сравнение | ==, != | Только slices.Equal |";

        var prompt =
            $"You are given subtitles from a technical talk titled \"{videoTitle}\".\n\n" +
            $"YOUR TASK: reproduce the entire talk as a structured reference article. This is NOT a summary — it is a faithful, detailed reconstruction of everything the speaker says.\n\n" +
            $"AUTHOR'S STYLE — match it exactly. Here is a concrete example of the required output style:\n\n" +
            $"---\n{styleExample}\n---\n\n" +
            $"STYLE RULES (derived from the example above):\n" +
            $"- Conversational but precise technical language — write exactly as a good lecturer speaks\n" +
            $"- Every concept gets: what it is, why it works this way, what happens under the hood\n" +
            $"- Comparisons with other languages when the speaker mentions them — include them fully\n" +
            $"- \"Many people mistakenly think...\", \"This is important\", \"Note that...\" — keep all such emphasis\n" +
            $"- Explain every nuance the speaker explains; if they say something twice for emphasis, reflect that emphasis\n\n" +
            $"SKIP ONLY:\n" +
            $"- Ads and sponsor segments\n" +
            $"- Subscribe / like / follow calls-to-action\n" +
            $"- Pure filler with zero informational content\n\n" +
            $"MUST INCLUDE:\n" +
            $"- Every technical explanation in full — if the speaker uses 5 sentences, write 5 sentences, not one\n" +
            $"- All code examples in fenced blocks with language tag and Russian inline comments; complete any truncated code to working state\n" +
            $"- All analogies, all \"why\", all \"under the hood\" explanations\n" +
            $"- All tips, anti-patterns, gotchas, edge cases, numbers, formulas\n\n" +
            $"OUTPUT FORMAT (Markdown only):\n" +
            $"- Major thematic blocks: ## ЧАСТЬ I. НАЗВАНИЕ\n" +
            $"- Numbered sections with timecodes: ### 1. Название [HH:MM – HH:MM]\n" +
            $"- Code: fenced blocks with language, Russian comments\n" +
            $"- Final section: ## ШПАРГАЛКА ДЛЯ СОБЕСА — tables and quick-reference rules\n\n" +
            $"IMPORTANT: all output text must be written in Russian.\n\n" +
            $"Subtitles:\n{subtitles}";

        var chatClient = new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(ApiKey),
            new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(15) });

        var response = await chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { MaxOutputTokenCount = 16384 },
            ct);

        var content = response.Value.Content[0].Text;
        return new YoutubeExtractResult(videoTitle, content);
    }
}
