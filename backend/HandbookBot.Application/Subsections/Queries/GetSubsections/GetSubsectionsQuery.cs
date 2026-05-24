using HandbookBot.Application.Subsections.Dtos;
using MediatR;

namespace HandbookBot.Application.Subsections.Queries.GetSubsections;

public sealed record GetSubsectionsQuery(Guid SectionId, long UserId) : IRequest<IReadOnlyList<SubsectionDto>>;
