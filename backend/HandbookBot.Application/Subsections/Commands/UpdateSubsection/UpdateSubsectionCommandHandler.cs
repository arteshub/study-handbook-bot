using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Subsections.Commands.UpdateSubsection;

internal sealed class UpdateSubsectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateSubsectionCommand>
{
    public async Task Handle(UpdateSubsectionCommand request, CancellationToken ct)
    {
        var subsection = await uow.Subsections.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Subsection), request.Id);

        var section = await uow.Sections.GetByIdAsync(subsection.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), subsection.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        subsection.Update(request.Title, request.Description);
        uow.Subsections.Update(subsection);
        await uow.SaveChangesAsync(ct);
    }
}
