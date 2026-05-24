using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Commands.UpdateSection;

internal sealed class UpdateSectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateSectionCommand>
{
    public async Task Handle(UpdateSectionCommand request, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Section), request.Id);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        section.Update(request.Title, request.Description, request.Icon);

        uow.Sections.Update(section);
        await uow.SaveChangesAsync(ct);
    }
}
