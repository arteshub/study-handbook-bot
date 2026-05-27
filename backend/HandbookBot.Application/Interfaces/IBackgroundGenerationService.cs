namespace HandbookBot.Application.Interfaces;

public record ActiveGenerationDto(Guid TopicId, string VideoTitle, int Progress);

public interface IBackgroundGenerationService
{
    bool TryStartGeneration(long userId, Guid topicId, string url, string videoTitle);
    IReadOnlyList<ActiveGenerationDto> GetActiveJobs(long userId);
}
