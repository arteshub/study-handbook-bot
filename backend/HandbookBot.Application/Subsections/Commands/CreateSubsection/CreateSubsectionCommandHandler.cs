using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Subsections.Commands.CreateSubsection;

internal sealed class CreateSubsectionCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateSubsectionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSubsectionCommand request, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(request.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), request.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        var existing = await uow.Subsections.GetBySectionIdAsync(request.SectionId, ct);
        var subsection = Subsection.Create(request.SectionId, request.Title, request.Description, existing.Count);

        await uow.Subsections.AddAsync(subsection, ct);
        await uow.SaveChangesAsync(ct);

        return subsection.Id;
    }
}
