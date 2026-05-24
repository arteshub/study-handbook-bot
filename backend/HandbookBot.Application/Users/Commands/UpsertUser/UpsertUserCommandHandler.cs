using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Users.Commands.UpsertUser;

internal sealed class UpsertUserCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpsertUserCommand>
{
    public async Task Handle(UpsertUserCommand request, CancellationToken ct)
    {
        var user = await uow.Users.GetByTelegramIdAsync(request.TelegramId, ct);

        if (user is null)
        {
            user = User.Create(request.TelegramId, request.FirstName, request.Username, request.LastName);
            await uow.Users.AddAsync(user, ct);
        }
        else
        {
            user.UpdateProfile(request.FirstName, request.Username, request.LastName);
            uow.Users.Update(user);
        }

        await uow.SaveChangesAsync(ct);
    }
}
