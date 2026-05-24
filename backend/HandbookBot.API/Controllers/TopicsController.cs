using HandbookBot.Application.Topics.Commands.CreateTopic;
using HandbookBot.Application.Topics.Commands.DeleteTopic;
using HandbookBot.Application.Topics.Commands.UpdateTopic;
using HandbookBot.Application.Topics.Queries.GetTopicById;
using HandbookBot.Application.Topics.Queries.GetTopics;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class TopicsController : BaseController
{
    [HttpGet("/api/subsections/{subsectionId:guid}/topics")]
    public async Task<IActionResult> GetAll(Guid subsectionId, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTopicsQuery(subsectionId, CurrentUserId), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) =>
        Ok(await Mediator.Send(new GetTopicByIdQuery(id, CurrentUserId), ct));

    [HttpPost("/api/subsections/{subsectionId:guid}/topics")]
    public async Task<IActionResult> Create(Guid subsectionId, [FromBody] CreateTopicRequest req, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateTopicCommand(subsectionId, CurrentUserId, req.Title, req.Content, req.Summary), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTopicRequest req, CancellationToken ct)
    {
        await Mediator.Send(new UpdateTopicCommand(id, CurrentUserId, req.Title, req.Content, req.Summary), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteTopicCommand(id, CurrentUserId), ct);
        return NoContent();
    }
}

public sealed record CreateTopicRequest(string Title, string Content, string? Summary);
public sealed record UpdateTopicRequest(string Title, string Content, string? Summary);
