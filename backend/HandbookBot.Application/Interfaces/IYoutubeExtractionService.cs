namespace HandbookBot.Application.Interfaces;

public record YoutubeExtractResult(string Title, string Content);

public interface IYoutubeExtractionService
{
    Task<string> GetVideoTitleAsync(string url, CancellationToken ct = default);
    Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, IProgress<int>? progress = null, CancellationToken ct = default);
}
