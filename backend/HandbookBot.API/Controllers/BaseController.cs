using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HandbookBot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected long CurrentUserId => long.Parse(User.FindFirst("telegram_id")?.Value
        ?? HttpContext.Items["TelegramUserId"]?.ToString()
        ?? throw new UnauthorizedAccessException("User not authenticated."));
}
