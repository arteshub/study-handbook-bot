using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Commands.CreateSection;

internal sealed class CreateSectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateSectionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSectionCommand request, CancellationToken ct)
    {
        var existing = await uow.Sections.GetByUserIdAsync(request.UserId, ct);
        var order = existing.Count;

        var section = Section.Create(request.UserId, request.Title, request.Description, request.Icon, order);

        await uow.Sections.AddAsync(section, ct);
        await uow.SaveChangesAsync(ct);

        return section.Id;
    }
}
