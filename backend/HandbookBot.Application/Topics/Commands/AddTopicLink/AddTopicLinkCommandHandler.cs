using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Application.Topics.Dtos;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.AddTopicLink;

internal sealed class AddTopicLinkCommandHandler(IUnitOfWork uow)
    : IRequestHandler<AddTopicLinkCommand, TopicLinkDto>
{
    public async Task<TopicLinkDto> Handle(AddTopicLinkCommand request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetWithChildrenAsync(request.TopicId, ct)
            ?? throw new NotFoundException(nameof(Topic), request.TopicId);

        var subsection = await uow.Subsections.GetByIdAsync(topic.SubsectionId, ct)!;
        var section = await uow.Sections.GetByIdAsync(subsection!.SectionId, ct)!;
        if (section!.UserId != request.UserId) throw new ForbiddenException();

        var link = topic.AddLink(request.Title, request.Url);
        await uow.SaveChangesAsync(ct);

        return new TopicLinkDto(link.Id, link.Title, link.Url);
    }
}
