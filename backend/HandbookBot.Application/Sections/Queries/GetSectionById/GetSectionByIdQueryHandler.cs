using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Sections.Dtos;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSectionById;

internal sealed class GetSectionByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetSectionByIdQuery, SectionDto>
{
    public async Task<SectionDto> Handle(GetSectionByIdQuery request, CancellationToken ct)
    {
        var section = await uow.Sections.GetWithSubsectionsAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Section), request.Id);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        return new SectionDto(
            section.Id,
            section.Title,
            section.Description,
            section.Icon,
            section.Order,
            section.Subsections.Count,
            section.Subsections.Sum(s => s.Topics.Count),
            section.CreatedAt,
            section.UpdatedAt);
    }
}
