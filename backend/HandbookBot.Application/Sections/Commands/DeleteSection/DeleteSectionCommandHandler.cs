using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Commands.DeleteSection;

internal sealed class DeleteSectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteSectionCommand>
{
    public async Task Handle(DeleteSectionCommand request, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Section), request.Id);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        uow.Sections.Remove(section);
        await uow.SaveChangesAsync(ct);
    }
}
