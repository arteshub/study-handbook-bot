using HandbookBot.Application.Sections.Dtos;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSections;

internal sealed class GetSectionsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetSectionsQuery, IReadOnlyList<SectionDto>>
{
    public async Task<IReadOnlyList<SectionDto>> Handle(GetSectionsQuery request, CancellationToken ct)
    {
        var sections = await uow.Sections.GetByUserIdAsync(request.UserId, ct);

        return sections
            .OrderBy(s => s.Order)
            .ThenBy(s => s.CreatedAt)
            .Select(s => new SectionDto(
                s.Id,
                s.Title,
                s.Description,
                s.Icon,
                s.Order,
                s.Subsections.Count,
                s.Subsections.Sum(sub => sub.Topics.Count),
                s.CreatedAt,
                s.UpdatedAt))
            .ToList();
    }
}
