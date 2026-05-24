using HandbookBot.Application.Sections.Commands.CreateSection;
using HandbookBot.Application.Sections.Commands.DeleteSection;
using HandbookBot.Application.Sections.Commands.UpdateSection;
using HandbookBot.Application.Sections.Queries.GetSectionById;
using HandbookBot.Application.Sections.Queries.GetSections;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class SectionsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await Mediator.Send(new GetSectionsQuery(CurrentUserId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetSectionByIdQuery(id, CurrentUserId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSectionRequest req, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateSectionCommand(CurrentUserId, req.Title, req.Description, req.Icon), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSectionRequest req, CancellationToken ct)
    {
        await Mediator.Send(new UpdateSectionCommand(id, CurrentUserId, req.Title, req.Description, req.Icon), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteSectionCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}

public sealed record CreateSectionRequest(string Title, string? Description, string? Icon);
public sealed record UpdateSectionRequest(string Title, string? Description, string? Icon);
