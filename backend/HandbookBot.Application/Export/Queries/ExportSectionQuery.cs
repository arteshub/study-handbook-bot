using HandbookBot.Application.Common.Interfaces;
using MediatR;

namespace HandbookBot.Application.Export.Queries;

public sealed record ExportSectionQuery(Guid SectionId, long UserId) : IRequest<byte[]>;

internal sealed class ExportSectionQueryHandler(IPdfService pdfService)
    : IRequestHandler<ExportSectionQuery, byte[]>
{
    public Task<byte[]> Handle(ExportSectionQuery request, CancellationToken ct) =>
        pdfService.ExportSectionAsync(request.SectionId, request.UserId, ct);
}

public sealed record ExportTopicQuery(Guid TopicId, long UserId) : IRequest<byte[]>;

internal sealed class ExportTopicQueryHandler(IPdfService pdfService)
    : IRequestHandler<ExportTopicQuery, byte[]>
{
    public Task<byte[]> Handle(ExportTopicQuery request, CancellationToken ct) =>
        pdfService.ExportTopicAsync(request.TopicId, ct);
}

public sealed record ExportAllQuery(long UserId) : IRequest<byte[]>;

internal sealed class ExportAllQueryHandler(IPdfService pdfService)
    : IRequestHandler<ExportAllQuery, byte[]>
{
    public Task<byte[]> Handle(ExportAllQuery request, CancellationToken ct) =>
        pdfService.ExportAllAsync(request.UserId, ct);
}
