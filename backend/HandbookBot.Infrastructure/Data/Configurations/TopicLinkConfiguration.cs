using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class TopicLinkConfiguration : IEntityTypeConfiguration<TopicLink>
{
    public void Configure(EntityTypeBuilder<TopicLink> builder)
    {
        builder.ToTable("topic_links");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title).HasMaxLength(500).IsRequired();
        builder.Property(l => l.Url).HasMaxLength(2000).IsRequired();
        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasIndex(l => l.TopicId);
    }
}
