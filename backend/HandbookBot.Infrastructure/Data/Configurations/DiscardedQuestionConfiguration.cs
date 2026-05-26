using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class DiscardedQuestionConfiguration : IEntityTypeConfiguration<DiscardedQuestion>
{
    public void Configure(EntityTypeBuilder<DiscardedQuestion> builder)
    {
        builder.ToTable("discarded_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.CachedQuestionId).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.CachedQuestionId }).IsUnique();
    }
}
