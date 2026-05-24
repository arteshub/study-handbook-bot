namespace HandbookBot.Application.Common.Interfaces;

public interface IPdfService
{
    Task<byte[]> ExportSectionAsync(Guid sectionId, long userId, CancellationToken ct = default);
    Task<byte[]> ExportTopicAsync(Guid topicId, CancellationToken ct = default);
    Task<byte[]> ExportAllAsync(long userId, CancellationToken ct = default);
}
