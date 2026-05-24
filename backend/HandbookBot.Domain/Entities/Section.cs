using HandbookBot.Domain.Common;
using HandbookBot.Domain.Exceptions;

namespace HandbookBot.Domain.Entities;

public sealed class Section : BaseEntity
{
    public long UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Icon { get; private set; } = "📚";
    public int Order { get; private set; }

    private readonly List<Subsection> _subsections = [];
    public IReadOnlyCollection<Subsection> Subsections => _subsections.AsReadOnly();

    private Section() { }

    public static Section Create(long userId, string title, string? description, string? icon, int order = 0)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Section title is required.");

        return new Section
        {
            UserId = userId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Icon = string.IsNullOrWhiteSpace(icon) ? "📚" : icon.Trim(),
            Order = order
        };
    }

    public void Update(string title, string? description, string? icon)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Section title is required.");
        Title = title.Trim();
        Description = description?.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? "📚" : icon.Trim();
        Touch();
    }

    public void Reorder(int order)
    {
        Order = order;
        Touch();
    }
}
