using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Infrastructure.Data;
using Markdig;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HandbookBot.Infrastructure.Services;

internal sealed class PdfService(AppDbContext db) : IPdfService
{
    public async Task<byte[]> ExportSectionAsync(Guid sectionId, long userId, CancellationToken ct = default)
    {
        var section = await db.Sections
            .Include(s => s.Subsections).ThenInclude(sub => sub.Topics)
            .FirstOrDefaultAsync(s => s.Id == sectionId && s.UserId == userId, ct)
            ?? throw new InvalidOperationException("Section not found.");

        return GeneratePdf(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text($"{section.Icon} {section.Title}").FontSize(24).Bold();
                    if (!string.IsNullOrEmpty(section.Description))
                        col.Item().Text(section.Description).FontSize(12).Italic();

                    col.Item().PaddingTop(10);

                    foreach (var sub in section.Subsections.OrderBy(s => s.Order))
                    {
                        col.Item().Text(sub.Title).FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                        foreach (var topic in sub.Topics.OrderBy(t => t.Order))
                        {
                            col.Item().PaddingTop(8).Text(topic.Title).FontSize(14).Bold();
                            col.Item().Text(Markdown.ToPlainText(topic.Content)).FontSize(11);
                            col.Item().PaddingBottom(8);
                        }
                    }
                });
            });
        });
    }

    public async Task<byte[]> ExportTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        var topic = await db.Topics.FirstOrDefaultAsync(t => t.Id == topicId, ct)
            ?? throw new InvalidOperationException("Topic not found.");

        return GeneratePdf(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text(topic.Title).FontSize(20).Bold();
                    col.Item().PaddingTop(10).Text(Markdown.ToPlainText(topic.Content)).FontSize(12);
                });
            });
        });
    }

    public async Task<byte[]> ExportAllAsync(long userId, CancellationToken ct = default)
    {
        var sections = await db.Sections
            .Include(s => s.Subsections).ThenInclude(sub => sub.Topics)
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.Order)
            .ToListAsync(ct);

        return GeneratePdf(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().Text("Справочник знаний").FontSize(28).Bold().AlignCenter();
                    col.Item().PaddingBottom(20);

                    foreach (var section in sections)
                    {
                        col.Item().Text($"{section.Icon} {section.Title}").FontSize(22).Bold();
                        foreach (var sub in section.Subsections.OrderBy(s => s.Order))
                        {
                            col.Item().PaddingTop(10).Text(sub.Title).FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                            foreach (var topic in sub.Topics.OrderBy(t => t.Order))
                            {
                                col.Item().PaddingTop(6).Text(topic.Title).FontSize(13).Bold();
                                col.Item().Text(Markdown.ToPlainText(topic.Content)).FontSize(11);
                            }
                        }
                        col.Item().PaddingBottom(15);
                    }
                });
            });
        });
    }

    private static byte[] GeneratePdf(Action<IDocumentContainer> compose)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var doc = Document.Create(compose);
        return doc.GeneratePdf();
    }
}
