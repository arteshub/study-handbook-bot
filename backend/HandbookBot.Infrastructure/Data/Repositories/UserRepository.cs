using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HandbookBot.Infrastructure.Data.Repositories;

internal sealed class UserRepository(AppDbContext db) : Repository<User>(db), IUserRepository
{
    public Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default) =>
        DbSet.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
}
