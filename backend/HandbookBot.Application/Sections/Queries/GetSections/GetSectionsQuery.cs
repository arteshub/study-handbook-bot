using HandbookBot.Application.Sections.Dtos;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSections;

public sealed record GetSectionsQuery(long UserId) : IRequest<IReadOnlyList<SectionDto>>;
