using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Topics.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Queries.GetTopics;

internal sealed class GetTopicsQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTopicsQuery, IReadOnlyList<TopicListItemDto>>
{
    public async Task<IReadOnlyList<TopicListItemDto>> Handle(GetTopicsQuery request, CancellationToken ct)
    {
        var subsection = await uow.Subsections.GetByIdAsync(request.SubsectionId, ct)
            ?? throw new NotFoundException(nameof(Subsection), request.SubsectionId);

        var section = await uow.Sections.GetByIdAsync(subsection.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), subsection.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        var topics = await uow.Topics.GetBySubsectionIdAsync(request.SubsectionId, ct);

        return topics
            .OrderBy(t => t.Order)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TopicListItemDto(t.Id, t.SubsectionId, t.Title, t.Summary, t.Order, t.UpdatedAt))
            .ToList();
    }
}
