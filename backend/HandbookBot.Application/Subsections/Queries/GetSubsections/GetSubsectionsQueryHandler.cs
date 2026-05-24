using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Subsections.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Subsections.Queries.GetSubsections;

internal sealed class GetSubsectionsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetSubsectionsQuery, IReadOnlyList<SubsectionDto>>
{
    public async Task<IReadOnlyList<SubsectionDto>> Handle(GetSubsectionsQuery request, CancellationToken ct)
    {
        var section = await uow.Sections.GetByIdAsync(request.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), request.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        var subsections = await uow.Subsections.GetBySectionIdAsync(request.SectionId, ct);

        return subsections
            .OrderBy(s => s.Order)
            .ThenBy(s => s.CreatedAt)
            .Select(s => new SubsectionDto(
                s.Id,
                s.SectionId,
                s.Title,
                s.Description,
                s.Order,
                s.Topics.Count,
                s.CreatedAt,
                s.UpdatedAt))
            .ToList();
    }
}
