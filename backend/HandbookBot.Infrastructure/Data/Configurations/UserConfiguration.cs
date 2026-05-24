using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TelegramId).HasColumnName("telegram_id").IsRequired();
        builder.HasIndex(u => u.TelegramId).IsUnique();
        builder.Property(u => u.Username).HasColumnName("username").HasMaxLength(100);
        builder.Property(u => u.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasColumnName("last_name").HasMaxLength(100);
        builder.Property(u => u.OpenAiApiKey).HasColumnName("openai_api_key");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(u => u.Sections)
            .WithOne()
            .HasForeignKey(s => s.UserId)
            .HasPrincipalKey(u => u.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
