using HandbookBot.Domain.Entities;

namespace HandbookBot.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
}
