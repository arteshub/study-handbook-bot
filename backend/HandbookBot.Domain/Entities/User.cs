using HandbookBot.Domain.Common;
using HandbookBot.Domain.Exceptions;

namespace HandbookBot.Domain.Entities;

public sealed class User : BaseEntity
{
    public long TelegramId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public string? OpenAiApiKey { get; private set; }

    private readonly List<Section> _sections = [];
    public IReadOnlyCollection<Section> Sections => _sections.AsReadOnly();

    private User() { }

    public static User Create(long telegramId, string firstName, string? username, string? lastName)
    {
        if (telegramId <= 0) throw new DomainException("Invalid Telegram ID.");
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required.");

        return new User
        {
            TelegramId = telegramId,
            FirstName = firstName.Trim(),
            Username = username?.Trim() ?? string.Empty,
            LastName = lastName?.Trim()
        };
    }

    public void UpdateProfile(string firstName, string? username, string? lastName)
    {
        FirstName = firstName.Trim();
        Username = username?.Trim() ?? string.Empty;
        LastName = lastName?.Trim();
        Touch();
    }

    public void SetOpenAiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) throw new DomainException("API key cannot be empty.");
        OpenAiApiKey = apiKey.Trim();
        Touch();
    }
}
