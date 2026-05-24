namespace HandbookBot.Domain.Entities;

public sealed class TopicLink
{
    public Guid Id { get; private set; }
    public Guid TopicId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private TopicLink() { }

    internal static TopicLink Create(Guid topicId, string title, string url) => new()
    {
        Id = Guid.NewGuid(),
        TopicId = topicId,
        Title = title.Trim(),
        Url = url.Trim(),
        CreatedAt = DateTime.UtcNow,
    };
}
