using HandbookBot.Application.Topics.Dtos;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Sections.Queries.GetSectionTree;

internal sealed class GetSectionTreeQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetSectionTreeQuery, IReadOnlyList<SectionTreeDto>>
{
    public async Task<IReadOnlyList<SectionTreeDto>> Handle(GetSectionTreeQuery request, CancellationToken ct)
    {
        var sections = await uow.Sections.GetByUserIdAsync(request.UserId, ct);
        var result = new List<SectionTreeDto>();

        foreach (var section in sections.OrderBy(s => s.Order))
        {
            var subsections = new List<SubsectionTreeDto>();
            foreach (var sub in section.Subsections.OrderBy(s => s.Order))
            {
                var roots = await uow.Topics.GetRootsBySubsectionIdAsync(sub.Id, ct);
                var topicNodes = await BuildTreeAsync(roots, ct);
                subsections.Add(new SubsectionTreeDto(sub.Id, sub.Title, topicNodes));
            }
            result.Add(new SectionTreeDto(section.Id, section.Title, section.Icon, subsections));
        }

        return result;
    }

    private async Task<IReadOnlyList<TopicTreeNodeDto>> BuildTreeAsync(IReadOnlyList<Topic> topics, CancellationToken ct)
    {
        var nodes = new List<TopicTreeNodeDto>();
        foreach (var topic in topics.OrderBy(t => t.Order))
        {
            var children = await uow.Topics.GetChildrenAsync(topic.Id, ct);
            var childNodes = await BuildTreeAsync(children, ct);
            nodes.Add(new TopicTreeNodeDto(topic.Id, topic.Title, topic.Summary, !string.IsNullOrEmpty(topic.Content), topic.Order, childNodes));
        }
        return nodes;
    }
}
