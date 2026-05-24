using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Subsections.Commands.DeleteSubsection;

internal sealed class DeleteSubsectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteSubsectionCommand>
{
    public async Task Handle(DeleteSubsectionCommand request, CancellationToken ct)
    {
        var subsection = await uow.Subsections.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Subsection), request.Id);

        var section = await uow.Sections.GetByIdAsync(subsection.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), subsection.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        uow.Subsections.Remove(subsection);
        await uow.SaveChangesAsync(ct);
    }
}
