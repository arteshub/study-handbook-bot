using HandbookBot.Application.Interfaces;
using HandbookBot.Application.YoutubeExtraction;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

[Route("api/youtube-extractor")]
public class YoutubeExtractorController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Extract([FromBody] string url, CancellationToken ct)
    {
        var result = await Mediator.Send(new YoutubeExtractCommand(url), ct);
        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartGeneration([FromBody] StartGenerationRequest req, CancellationToken ct)
    {
        var result = await Mediator.Send(new StartYoutubeGenerationCommand(req.Url, req.SubsectionId, CurrentUserId), ct);
        return Ok(new { result.TopicId, result.VideoTitle });
    }

    [HttpGet("active")]
    public IActionResult GetActiveJobs([FromServices] IBackgroundGenerationService backgroundService)
    {
        return Ok(backgroundService.GetActiveJobs(CurrentUserId));
    }
}

public sealed record StartGenerationRequest(string Url, Guid SubsectionId);
