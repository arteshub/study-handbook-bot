using System.ClientModel;
using System.Text;
using HandbookBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using YoutubeExplode;

namespace HandbookBot.Infrastructure.Services;

public class YoutubeExtractionService(YoutubeClient youtubeClient, IConfiguration config) : IYoutubeExtractionService
{
    private string ApiKey => config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

    public async Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, CancellationToken ct = default)
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

        var prompt =
            $"You are given subtitles from a technical talk titled \"{videoTitle}\".\n\n" +
            $"YOUR TASK: produce a verbatim structured transcript — not a summary, not a rewrite. Reproduce every explanation the speaker gives in full detail. If the speaker spends 5 sentences on something, write 5 sentences, not one.\n\n" +
            $"SKIP ONLY:\n" +
            $"- Ads and sponsor segments\n" +
            $"- Subscribe / like / follow calls-to-action\n" +
            $"- Filler words and meaningless repetition\n\n" +
            $"MUST INCLUDE EVERYTHING ELSE:\n" +
            $"- Every technical explanation, fully and precisely\n" +
            $"- All code examples in fenced code blocks with language tag and inline comments; if the subtitle text contains incomplete or truncated code, complete it to a working state\n" +
            $"- All analogies and comparisons to other languages or approaches\n" +
            $"- All \"why\", \"under the hood\", \"what actually happens\" explanations\n" +
            $"- All tips, anti-patterns, gotchas\n" +
            $"- All numbers, formulas, edge cases\n\n" +
            $"OUTPUT FORMAT (Markdown only):\n" +
            $"- Major thematic blocks: ## ЧАСТЬ I. НАЗВАНИЕ\n" +
            $"- Numbered sections with timecodes: ### 1. Название [HH:MM – HH:MM]\n" +
            $"- Code: fenced blocks with language, comments in Russian\n" +
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
