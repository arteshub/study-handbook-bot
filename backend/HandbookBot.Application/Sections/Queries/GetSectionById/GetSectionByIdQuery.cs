using HandbookBot.Application.Sections.Dtos;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSectionById;

public sealed record GetSectionByIdQuery(Guid Id, long UserId) : IRequest<SectionDto>;
