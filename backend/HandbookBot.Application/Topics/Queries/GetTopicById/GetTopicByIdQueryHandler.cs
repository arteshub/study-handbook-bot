using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Topics.Dtos;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Queries.GetTopicById;

internal sealed class GetTopicByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetTopicByIdQuery, TopicDto>
{
    public async Task<TopicDto> Handle(GetTopicByIdQuery request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetWithChildrenAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Topic), request.Id);

        var subsection = await uow.Subsections.GetByIdAsync(topic.SubsectionId, ct)!;
        var section = await uow.Sections.GetByIdAsync(subsection!.SectionId, ct)!;

        if (section!.UserId != request.UserId) throw new ForbiddenException();

        return new TopicDto(topic.Id, topic.SubsectionId, topic.ParentTopicId, topic.Title, topic.Content, topic.Summary, topic.Order, topic.Children.Count, topic.CreatedAt, topic.UpdatedAt);
    }
}
