using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Users.Commands.SetOpenAiKey;

internal sealed class SetOpenAiKeyCommandHandler(IUnitOfWork uow)
    : IRequestHandler<SetOpenAiKeyCommand>
{
    public async Task Handle(SetOpenAiKeyCommand request, CancellationToken ct)
    {
        var user = await uow.Users.GetByTelegramIdAsync(request.TelegramId, ct)
            ?? throw new NotFoundException(nameof(User), request.TelegramId);

        user.SetOpenAiKey(request.ApiKey);
        uow.Users.Update(user);
        await uow.SaveChangesAsync(ct);
    }
}
