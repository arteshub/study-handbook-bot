using HandbookBot.Application.Export.Queries;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class ExportController : BaseController
{
    [HttpGet("sections/{sectionId:guid}")]
    public async Task<IActionResult> ExportSection(Guid sectionId, CancellationToken ct)
    {
        var pdf = await Mediator.Send(new ExportSectionQuery(sectionId, CurrentUserId), ct);
        return File(pdf, "application/pdf", "section.pdf");
    }

    [HttpGet("topics/{topicId:guid}")]
    public async Task<IActionResult> ExportTopic(Guid topicId, CancellationToken ct)
    {
        var pdf = await Mediator.Send(new ExportTopicQuery(topicId, CurrentUserId), ct);
        return File(pdf, "application/pdf", "topic.pdf");
    }

    [HttpGet("all")]
    public async Task<IActionResult> ExportAll(CancellationToken ct)
    {
        var pdf = await Mediator.Send(new ExportAllQuery(CurrentUserId), ct);
        return File(pdf, "application/pdf", "handbook.pdf");
    }
}
