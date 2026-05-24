using HandbookBot.Application.Topics.Dtos;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSectionTree;

public sealed record GetSectionTreeQuery(long UserId) : IRequest<IReadOnlyList<SectionTreeDto>>;
