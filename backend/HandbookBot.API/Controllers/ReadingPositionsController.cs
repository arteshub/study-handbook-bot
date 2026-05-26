using HandbookBot.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class ReadingPositionsController(IUnitOfWork uow) : BaseController
{
    [HttpGet("{topicId:guid}")]
    public async Task<IActionResult> Get(Guid topicId, CancellationToken ct)
    {
        var pos = await uow.ReadingPositions.GetAsync(CurrentUserId, topicId, ct);
        return Ok(new { scrollRatio = pos?.ScrollRatio ?? 0f });
    }
}
