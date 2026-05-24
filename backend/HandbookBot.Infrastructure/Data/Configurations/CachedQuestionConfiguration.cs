using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class CachedQuestionConfiguration : IEntityTypeConfiguration<CachedQuestion>
{
    public void Configure(EntityTypeBuilder<CachedQuestion> builder)
    {
        builder.ToTable("cached_questions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TopicId).IsRequired();
        builder.Property(x => x.Mode).IsRequired().HasConversion<string>();
        builder.Property(x => x.ContentHash).IsRequired().HasMaxLength(64);
        builder.Property(x => x.QuestionText).IsRequired();
        builder.Property(x => x.ModelAnswer);
        builder.Property(x => x.OptionsJson);
        builder.HasIndex(x => new { x.TopicId, x.Mode, x.ContentHash });
    }
}
