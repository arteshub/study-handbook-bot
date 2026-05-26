using System.ClientModel;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HandbookBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace HandbookBot.Infrastructure.Services;

public class YoutubeExtractionService(IConfiguration config) : IYoutubeExtractionService
{
    private string ApiKey => config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

    private const int ChunkMinutes = 10;

    // Android client — YouTube cannot block it without breaking their own app
    private static readonly HttpClient Http = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "com.google.android.youtube/19.09.37 (Linux; U; Android 11) gzip" },
            { "X-Youtube-Client-Name", "3" },
            { "X-Youtube-Client-Version", "19.09.37" },
        }
    };

    public async Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, CancellationToken ct = default)
    {
        var videoId = ExtractVideoId(url)
            ?? throw new InvalidOperationException("Не удалось извлечь ID видео из ссылки.");

        var (title, subtitles) = await FetchSubtitlesAsync(videoId, ct);

        var lines = subtitles.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        var chunks = SplitIntoChunks(lines, ChunkMinutes);

        var client = CreateChatClient();
        var parts = new List<string>();
        for (int i = 0; i < chunks.Count; i++)
            parts.Add(await GenerateChunkAsync(client, title, chunks[i], i + 1, chunks.Count, ct));

        return new YoutubeExtractResult(title, string.Join("\n\n", parts));
    }

    private static async Task<(string Title, string Subtitles)> FetchSubtitlesAsync(string videoId, CancellationToken ct)
    {
        var body = new
        {
            context = new
            {
                client = new
                {
                    clientName = "ANDROID",
                    clientVersion = "19.09.37",
                    androidSdkVersion = 30,
                    hl = "ru",
                    gl = "US"
                }
            },
            videoId,
            contentCheckOk = true,
            racyCheckOk = true
        };

        var response = await Http.PostAsJsonAsync(
            "https://www.youtube.com/youtubei/v1/player", body, ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

        // video title
        var title = videoId;
        if (json.TryGetProperty("videoDetails", out var details) &&
            details.TryGetProperty("title", out var t))
            title = t.GetString() ?? videoId;

        // find caption tracks
        if (!json.TryGetProperty("captions", out var captions) ||
            !captions.TryGetProperty("playerCaptionsTracklistRenderer", out var renderer) ||
            !renderer.TryGetProperty("captionTracks", out var tracks))
            throw new InvalidOperationException("Видео не содержит субтитров.");

        var trackList = tracks.EnumerateArray().ToList();
        if (trackList.Count == 0)
            throw new InvalidOperationException("Видео не содержит субтитров.");

        // prefer Russian, fall back to first
        var track = trackList.FirstOrDefault(t =>
            t.TryGetProperty("languageCode", out var lc) &&
            lc.GetString()?.StartsWith("ru", StringComparison.OrdinalIgnoreCase) == true);
        if (track.ValueKind == JsonValueKind.Undefined)
            track = trackList[0];

        var baseUrl = track.GetProperty("baseUrl").GetString()!;
        var captionsJson = await Http.GetFromJsonAsync<JsonElement>(baseUrl + "&fmt=json3", ct);

        var sb = new StringBuilder();
        foreach (var evt in captionsJson.GetProperty("events").EnumerateArray())
        {
            if (!evt.TryGetProperty("segs", out var segs)) continue;
            var text = string.Concat(segs.EnumerateArray()
                .Select(s => s.TryGetProperty("utf8", out var u) ? u.GetString() ?? "" : ""))
                .Trim();
            if (string.IsNullOrWhiteSpace(text) || text == "\n") continue;

            var tMs = evt.GetProperty("tStartMs").GetInt64();
            sb.AppendLine($"[{tMs / 60000:D2}:{tMs % 60000 / 1000:D2}] {text}");
        }

        return (title, sb.ToString());
    }

    private static string? ExtractVideoId(string url)
    {
        var m = Regex.Match(url, @"[?&]v=([^&]+)");
        if (m.Success) return m.Groups[1].Value;
        m = Regex.Match(url, @"youtu\.be/([^?&]+)");
        if (m.Success) return m.Groups[1].Value;
        return null;
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
            $"YOUR TASK: produce a verbatim structured transcript of this segment — not a summary, not a rewrite. Reproduce every explanation the speaker gives in full detail. If the speaker spends 5 sentences on something, write 5 sentences, not one.\n\n" +
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
            $"FORMAT:\n" +
            $"- Number sections continuing from where segment {partIndex - 1} left off (start from 1 only if this is segment 1)\n" +
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

    private ChatClient CreateChatClient() => new(
        "gpt-4o-mini",
        new ApiKeyCredential(ApiKey),
        new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(15) });
}
