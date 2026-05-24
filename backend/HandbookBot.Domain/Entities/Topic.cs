using HandbookBot.Domain.Common;
using HandbookBot.Domain.Exceptions;

namespace HandbookBot.Domain.Entities;

public sealed class Topic : BaseEntity
{
    public Guid SubsectionId { get; private set; }
    public Guid? ParentTopicId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public int Order { get; private set; }

    private readonly List<Topic> _children = [];
    public IReadOnlyCollection<Topic> Children => _children.AsReadOnly();

    private Topic() { }

    public static Topic Create(Guid subsectionId, string title, string content, string? summary, int order = 0, Guid? parentTopicId = null)
    {
        if (subsectionId == Guid.Empty) throw new DomainException("Subsection ID is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Topic title is required.");

        return new Topic
        {
            SubsectionId = subsectionId,
            ParentTopicId = parentTopicId,
            Title = title.Trim(),
            Content = content.Trim(),
            Summary = summary?.Trim(),
            Order = order
        };
    }

    public void Update(string title, string content, string? summary)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Topic title is required.");
        Title = title.Trim();
        Content = content.Trim();
        Summary = summary?.Trim();
        Touch();
    }

    public void Reorder(int order) { Order = order; Touch(); }
}
