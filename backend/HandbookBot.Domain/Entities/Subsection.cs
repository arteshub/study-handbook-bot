using HandbookBot.Domain.Common;
using HandbookBot.Domain.Exceptions;

namespace HandbookBot.Domain.Entities;

public sealed class Subsection : BaseEntity
{
    public Guid SectionId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Order { get; private set; }

    private readonly List<Topic> _topics = [];
    public IReadOnlyCollection<Topic> Topics => _topics.AsReadOnly();

    private Subsection() { }

    public static Subsection Create(Guid sectionId, string title, string? description, int order = 0)
    {
        if (sectionId == Guid.Empty) throw new DomainException("Section ID is required.");
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Subsection title is required.");

        return new Subsection
        {
            SectionId = sectionId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Order = order
        };
    }

    public void Update(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Subsection title is required.");
        Title = title.Trim();
        Description = description?.Trim();
        Touch();
    }

    public void Reorder(int order)
    {
        Order = order;
        Touch();
    }
}
