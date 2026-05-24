using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HandbookBot.Infrastructure.Data.Configurations;

internal sealed class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
{
    public void Configure(EntityTypeBuilder<TestSession> builder)
    {
        builder.ToTable("test_sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(s => s.Mode).HasColumnName("mode").HasConversion<string>().IsRequired();
        builder.Property(s => s.SectionId).HasColumnName("section_id");
        builder.Property(s => s.SubsectionId).HasColumnName("subsection_id");
        builder.Property(s => s.TopicId).HasColumnName("topic_id");
        builder.Property(s => s.TotalQuestions).HasColumnName("total_questions");
        builder.Property(s => s.CorrectAnswers).HasColumnName("correct_answers");
        builder.Property(s => s.CompletedAt).HasColumnName("completed_at");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(s => s.Results).WithOne().HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}
