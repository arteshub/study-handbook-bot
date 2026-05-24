using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Common.Interfaces;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Chat;

internal sealed class ChatCommandHandler(IUnitOfWork uow, IAiService aiService)
    : IRequestHandler<ChatCommand, string>
{
    public async Task<string> Handle(ChatCommand request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetByIdAsync(request.TopicId, ct)
            ?? throw new NotFoundException(nameof(Topic), request.TopicId);

        var history = request.History
            .Select(m => (m.Role, m.Content))
            .ToList();

        return await aiService.ChatAsync(
            topic.Title,
            topic.Content,
            request.QuestionContext,
            request.ModelAnswerContext,
            history,
            request.Message,
            ct);
    }
}
