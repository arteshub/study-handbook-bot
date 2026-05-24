using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class TopicProgressConfiguration : IEntityTypeConfiguration<TopicProgress>
{
    public void Configure(EntityTypeBuilder<TopicProgress> builder)
    {
        builder.ToTable("topic_progress");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TopicId).IsRequired();
        builder.Property(x => x.EaseFactor).IsRequired();
        builder.Property(x => x.IntervalDays).IsRequired();
        builder.Property(x => x.NextReviewAt).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.TopicId }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.NextReviewAt });
    }
}
