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
        string videoTitle;
        try
        {
            var video = await youtubeClient.Videos.GetAsync(url, ct);
            videoTitle = video.Title;
        }
        catch (YoutubeExplodeException)
        {
            // Watch page may be blocked from datacenter IPs; use URL as fallback title
            var match = System.Text.RegularExpressions.Regex.Match(url, @"[?&]v=([^&]+)");
            videoTitle = match.Success ? $"YouTube video ({match.Groups[1].Value})" : "YouTube video";
        }

        var manifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(url, ct);
        var trackInfo = manifest.Tracks
            .FirstOrDefault(t => t.Language.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            ?? manifest.Tracks.FirstOrDefault()
            ?? throw new InvalidOperationException("Видео не содержит субтитров.");

        var track = await youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo, ct);

        var lines = new List<string>();
        foreach (var caption in track.Captions)
        {
            var offset = caption.Offset;
            lines.Add($"[{(int)offset.TotalMinutes:D2}:{offset.Seconds:D2}] {caption.Text}");
        }

        var chunks = SplitIntoChunks(lines, ChunkMinutes);
        var client = CreateClient();

        var parts = new List<string>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var part = await GenerateChunkAsync(client, videoTitle, chunks[i], i + 1, chunks.Count, ct);
            parts.Add(part);
        }

        var content = string.Join("\n\n", parts);
        return new YoutubeExtractResult(videoTitle, content);
    }

    private static List<string> SplitIntoChunks(List<string> lines, int chunkMinutes)
    {
        var chunks = new List<string>();
        var current = new StringBuilder();
        int chunkStart = 0;

        foreach (var line in lines)
        {
            // parse [MM:SS] at start of each line
            if (line.Length >= 7 && line[0] == '[')
            {
                var colon = line.IndexOf(':');
                var close = line.IndexOf(']');
                if (colon > 0 && close > colon &&
                    int.TryParse(line[1..colon], out int mins))
                {
                    if (mins >= chunkStart + chunkMinutes && current.Length > 0)
                    {
                        chunks.Add(current.ToString().TrimEnd());
                        current.Clear();
                        chunkStart = mins;
                    }
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
            "Индексация начинается с нуля. Слева и справа от массива в памяти — чужие данные, не принадлежащие массиву.\n\n" +
            "### 2. Создание массива [01:46 – 02:49]\n\n" +
            "```go\n" +
            "// 1) Просто объявление - zero value у int это 0\n" +
            "var a [5]int\n\n" +
            "// 2) С инициализацией части - остальные элементы zero value\n" +
            "b := [5]int{1, 2, 3}  // [1 2 3 0 0]\n" +
            "```\n\n" +
            "Ключевой принцип: массив всегда инициализируется zero value. У int это 0.";

        var isLast = partIndex == totalParts;
        var cheatsheetInstruction = isLast
            ? "After the last section, add a final ## ШПАРГАЛКА ДЛЯ СОБЕСА block with structured tables and quick-reference rules summarising the key points from the ENTIRE talk (not just this segment).\n\n"
            : "Do NOT generate a шпаргалка — it will be added after all parts are processed.\n\n";

        var prompt =
            $"You are given subtitles for segment {partIndex} of {totalParts} of the technical talk \"{videoTitle}\".\n\n" +
            $"YOUR TASK: reproduce this segment as a structured article — NOT a summary. Reproduce every explanation the speaker gives in full detail. If the speaker uses 5 sentences, write 5 sentences.\n\n" +
            $"AUTHOR'S STYLE — match it exactly. Example:\n\n---\n{styleExample}\n---\n\n" +
            $"STYLE RULES:\n" +
            $"- Conversational but precise — write exactly as a good lecturer speaks\n" +
            $"- Every concept: what it is, why it works this way, what happens under the hood\n" +
            $"- Include all comparisons with other languages, all analogies, all emphasis (\"Many people mistakenly think...\", \"This is important\")\n" +
            $"- Every nuance the speaker explains must appear in the output\n\n" +
            $"SKIP ONLY: ads, sponsor segments, subscribe/like calls-to-action, pure filler.\n\n" +
            $"MUST INCLUDE EVERYTHING ELSE:\n" +
            $"- Every technical explanation in full\n" +
            $"- All code in fenced blocks with language tag and Russian inline comments; complete any truncated code to working state\n" +
            $"- All tips, anti-patterns, gotchas, edge cases, numbers, formulas\n\n" +
            $"FORMAT:\n" +
            $"- Number sections continuing from where segment {partIndex - 1} left off (start section numbering fresh only if this is segment 1)\n" +
            $"- Sections: ### N. Название [HH:MM – HH:MM]\n" +
            $"- Use ## ЧАСТЬ headers for major thematic shifts\n" +
            $"{cheatsheetInstruction}" +
            $"IMPORTANT: all output text must be written in Russian.\n\n" +
            $"Subtitles (segment {partIndex}/{totalParts}):\n{subtitleChunk}";

        var response = await client.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { MaxOutputTokenCount = 16384 },
            ct);

        return response.Value.Content[0].Text;
    }

    private ChatClient CreateClient() => new(
        "gpt-4o-mini",
        new ApiKeyCredential(ApiKey),
        new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(15) });
}
