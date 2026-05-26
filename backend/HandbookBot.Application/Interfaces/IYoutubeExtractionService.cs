namespace HandbookBot.Application.Interfaces;

public record YoutubeExtractResult(string Title, string Content);

public interface IYoutubeExtractionService
{
    Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, CancellationToken ct = default);
}
