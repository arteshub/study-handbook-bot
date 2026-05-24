using HandbookBot.Application.Users.Commands.SetOpenAiKey;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class UsersController : BaseController
{
    [HttpPut("openai-key")]
    public async Task<IActionResult> SetOpenAiKey([FromBody] SetKeyRequest req, CancellationToken ct)
    {
        await Mediator.Send(new SetOpenAiKeyCommand(CurrentUserId, req.ApiKey), ct);
        return NoContent();
    }
}

public sealed record SetKeyRequest(string ApiKey);
