using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DebugController : ControllerBase
{
    [HttpPost("log")]
    public IActionResult Log([FromBody] DebugLogRequest req)
    {
        Console.WriteLine($"[FRONTEND] {req.Msg}");
        return Ok();
    }
}

public sealed record DebugLogRequest(string Msg);
