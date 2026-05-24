using HandbookBot.Application.Subsections.Commands.CreateSubsection;
using HandbookBot.Application.Subsections.Commands.DeleteSubsection;
using HandbookBot.Application.Subsections.Commands.UpdateSubsection;
using HandbookBot.Application.Subsections.Queries.GetSubsections;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class SubsectionsController : BaseController
{
    [HttpGet("/api/sections/{sectionId:guid}/subsections")]
    public async Task<IActionResult> GetAll(Guid sectionId, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetSubsectionsQuery(sectionId, CurrentUserId), ct));

    [HttpPost("/api/sections/{sectionId:guid}/subsections")]
    public async Task<IActionResult> Create(Guid sectionId, [FromBody] SubsectionRequest req, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateSubsectionCommand(sectionId, CurrentUserId, req.Title, req.Description), ct);
        return Created($"/api/subsections/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SubsectionRequest req, CancellationToken ct)
    {
        await Mediator.Send(new UpdateSubsectionCommand(id, CurrentUserId, req.Title, req.Description), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteSubsectionCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}

public sealed record SubsectionRequest(string Title, string? Description);
