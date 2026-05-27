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

    private const int ChunkMinutes = 10;

    public async Task<string> GetVideoTitleAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var video = await youtubeClient.Videos.GetAsync(url, ct);
            return video.Title;
        }
        catch (YoutubeExplodeException)
        {
            var match = System.Text.RegularExpressions.Regex.Match(url, @"[?&]v=([^&]+)");
            return match.Success ? $"YouTube video ({match.Groups[1].Value})" : "YouTube video";
        }
    }

    public async Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, IProgress<int>? progress = null, CancellationToken ct = default)
    {
        try
        {
            return await ExtractInternalAsync(url, progress, ct);
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

    private async Task<YoutubeExtractResult> ExtractInternalAsync(string url, IProgress<int>? progress, CancellationToken ct)
    {
        string videoTitle;
        try
        {
            var video = await youtubeClient.Videos.GetAsync(url, ct);
            videoTitle = video.Title;
        }
        catch (YoutubeExplodeException)
        {
            var match = System.Text.RegularExpressions.Regex.Match(url, @"[?&]v=([^&]+)");
            videoTitle = match.Success ? $"YouTube video ({match.Groups[1].Value})" : "YouTube video";
        }
        progress?.Report(5);

        var manifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(url, ct);
        var trackInfo = manifest.Tracks
            .FirstOrDefault(t => t.Language.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            ?? manifest.Tracks.FirstOrDefault()
            ?? throw new InvalidOperationException("Видео не содержит субтитров.");
        progress?.Report(10);

        var track = await youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo, ct);
        progress?.Report(15);

        var lines = new List<string>();
        foreach (var caption in track.Captions)
        {
            var offset = caption.Offset;
            lines.Add($"[{(int)offset.TotalMinutes:D2}:{offset.Seconds:D2}] {caption.Text}");
        }

        var chunks = SplitIntoChunks(lines, ChunkMinutes);
        var client = CreateChatClient();

        var parts = new List<string>();
        for (int i = 0; i < chunks.Count; i++)
        {
            parts.Add(await GenerateChunkAsync(client, videoTitle, chunks[i], i + 1, chunks.Count, ct));
            progress?.Report(15 + (int)((i + 1.0) / chunks.Count * 80));
        }

        progress?.Report(100);
        return new YoutubeExtractResult(videoTitle, string.Join("\n\n", parts.Where(p => !string.IsNullOrWhiteSpace(p))));
    }

    private static List<string> SplitIntoChunks(List<string> lines, int chunkMinutes)
    {
        var chunks = new List<string>();
        var current = new StringBuilder();
        int chunkStart = 0;

        foreach (var line in lines)
        {
            if (line.Length >= 7 && line[0] == '[')
            {
                var colon = line.IndexOf(':');
                var close = line.IndexOf(']');
                if (colon > 0 && close > colon &&
                    int.TryParse(line[1..colon], out int mins) &&
                    mins >= chunkStart + chunkMinutes &&
                    current.Length > 0)
                {
                    chunks.Add(current.ToString().TrimEnd());
                    current.Clear();
                    chunkStart = mins;
                }
            }
            current.AppendLine(line);
        }

        if (current.Length > 0)
            chunks.Add(current.ToString().TrimEnd());

        return chunks;
    }

    private static async Task<string> GenerateChunkAsync(
        ChatClient client, string videoTitle, string subtitleChunk,
        int partIndex, int totalParts, CancellationToken ct)
    {
        const string styleExample =
            "## ЧАСТЬ I. МАССИВЫ\n\n" +
            "### 1. Что такое массив [00:42 – 01:46]\n\n" +
            "Массив — структура данных фиксированного размера, которая хранит элементы последовательно в памяти (это критично важно — именно последовательность даёт O(1) доступ по индексу). Все элементы одного типа.\n\n" +
            "У каждого элемента есть:\n\n" +
            "- адрес — место в памяти\n" +
            "- размер — сколько байт занимает\n\n" +
            "Индексация начинается с нуля. Слева и справа от массива в памяти — чужие данные.\n\n" +
            "### 2. Создание массива [01:46 – 02:49]\n\n" +
            "```go\n" +
            "// 1) Просто объявление - zero value у int это 0\n" +
            "var a [5]int\n\n" +
            "// 2) С инициализацией части - остальные элементы zero value\n" +
            "b := [5]int{1, 2, 3}  // [1 2 3 0 0]\n" +
            "```\n\n" +
            "Ключевой принцип: массив всегда инициализируется zero value. У int это 0.";

        var cheatsheetInstruction = partIndex == totalParts
            ? "After the last section, add ## ШПАРГАЛКА ДЛЯ СОБЕСА with structured tables and quick-reference rules covering the ENTIRE talk.\n\n"
            : "Do NOT add a шпаргалка — it will be added in the final segment.\n\n";

        var prompt =
            $"You are given subtitles for segment {partIndex} of {totalParts} of the technical talk \"{videoTitle}\".\n\n" +
            $"=== RULE 1 — SKIP LIST (hard rule, no exceptions) ===\n" +
            $"Completely omit anything that falls into these categories — do not mention, summarize, or reference them:\n" +
            $"- Any promotion of a paid course, product, or service (\"мой курс\", \"запишитесь\", \"5 недель\", \"поток\", \"купить\")\n" +
            $"- Sponsor segments and advertisements\n" +
            $"- Subscribe/like/follow/share calls-to-action\n" +
            $"- Speaker biography or self-introduction filler\n" +
            $"- Pure audience interaction with zero technical content (\"напишите в чат\", \"кто хочет\")\n\n" +
            $"If an entire section consists only of promotional content, skip the section entirely.\n\n" +
            $"=== RULE 2 — CODE (hard rule, mandatory for every concept) ===\n" +
            $"Subtitles do not contain code — the speaker shows code on screen while talking. You MUST reconstruct every piece of code the speaker demonstrates.\n" +
            $"For EVERY programming concept, pattern, or function the speaker explains: write a complete, runnable implementation in a fenced code block.\n" +
            $"Rules for code blocks:\n" +
            $"- Language tag on the fence (```go, ```python, etc.)\n" +
            $"- Russian inline comments on every non-obvious line\n" +
            $"- Complete and compilable — no \"...\" placeholders, no pseudocode\n" +
            $"- If the speaker verbally describes a function signature, parameters, or algorithm — write the full implementation\n" +
            $"A section that discusses code but contains no code block is WRONG. Every pattern section must have at least one code example.\n\n" +
            $"=== RULE 3 — CONTENT COMPLETENESS ===\n" +
            $"Reproduce every technical explanation the speaker gives in full detail — not a summary. If the speaker uses 5 sentences to explain something, write 5 sentences.\n" +
            $"Include: every \"why\", every \"under the hood\", all analogies, all gotchas, all anti-patterns, all tips, all numbers/formulas.\n\n" +
            $"=== STYLE — match this example exactly ===\n\n" +
            $"---\n{styleExample}\n---\n\n" +
            $"Notice how the example has prose explanation followed by a code block — this is the required pattern for every concept.\n\n" +
            $"=== FORMAT ===\n" +
            $"- Sections: ### N. Название [HH:MM – HH:MM] (continue numbering from segment {partIndex - 1}; start at 1 only for segment 1)\n" +
            $"- Use ## ЧАСТЬ headers for major thematic shifts\n" +
            $"- Conversational but precise technical Russian throughout\n" +
            $"{cheatsheetInstruction}" +
            $"All output text must be in Russian.\n\n" +
            $"Subtitles (segment {partIndex}/{totalParts}):\n{subtitleChunk}";

        var response = await client.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { MaxOutputTokenCount = 16384 },
            ct);

        var text = response.Value.Content[0].Text;
        // OpenAI sometimes refuses ad-heavy chunks — return empty string so the segment is silently skipped
        if (text.StartsWith("I'm sorry", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("I cannot", StringComparison.OrdinalIgnoreCase) ||
            text.StartsWith("I'm unable", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return text;
    }

    private ChatClient CreateChatClient() => new(
        "gpt-4o-mini",
        new ApiKeyCredential(ApiKey),
        new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(15) });
}
