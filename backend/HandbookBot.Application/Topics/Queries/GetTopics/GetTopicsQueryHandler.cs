using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Topics.Dtos;
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

        var topics = request.ParentTopicId.HasValue
            ? await uow.Topics.GetChildrenAsync(request.ParentTopicId.Value, ct)
            : await uow.Topics.GetRootsBySubsectionIdAsync(request.SubsectionId, ct);

        return topics
            .Select(t => new TopicListItemDto(t.Id, t.SubsectionId, t.ParentTopicId, t.Title, t.Summary, t.Order, t.Children.Count, t.UpdatedAt))
            .ToList();
    }
}
