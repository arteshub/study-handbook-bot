using HandbookBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class ReadingPositionConfiguration : IEntityTypeConfiguration<ReadingPosition>
{
    public void Configure(EntityTypeBuilder<ReadingPosition> builder)
    {
        builder.ToTable("reading_positions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TopicId).IsRequired();
        builder.Property(x => x.ScrollRatio).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.TopicId }).IsUnique();
    }
}
