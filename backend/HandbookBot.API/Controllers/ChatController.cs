using HandbookBot.Application.Chat;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

public sealed class ChatController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req, CancellationToken ct)
    {
        var reply = await Mediator.Send(new ChatCommand(
            req.TopicId,
            req.QuestionContext,
            req.ModelAnswerContext,
            req.History.Select(m => new Application.Chat.ChatMessage(m.Role, m.Content)).ToList(),
            req.Message), ct);
        return Ok(new { reply });
    }
}

public sealed record ChatRequest(
    Guid TopicId,
    string QuestionContext,
    string ModelAnswerContext,
    List<ChatMessageDto> History,
    string Message);

public sealed record ChatMessageDto(string Role, string Content);
