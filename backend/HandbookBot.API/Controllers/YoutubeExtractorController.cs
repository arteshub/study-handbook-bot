using HandbookBot.Application.YoutubeExtraction;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public class YoutubeExtractorController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Extract([FromBody] string url, CancellationToken ct)
    {
        var result = await Mediator.Send(new YoutubeExtractCommand(url), ct);
        return Ok(result);
    }
}
