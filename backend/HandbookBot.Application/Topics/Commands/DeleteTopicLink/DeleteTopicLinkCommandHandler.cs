using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.DeleteTopicLink;

internal sealed class DeleteTopicLinkCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteTopicLinkCommand>
{
    public async Task Handle(DeleteTopicLinkCommand request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetWithChildrenAsync(request.TopicId, ct)
            ?? throw new NotFoundException(nameof(Topic), request.TopicId);

        var subsection = await uow.Subsections.GetByIdAsync(topic.SubsectionId, ct)!;
        var section = await uow.Sections.GetByIdAsync(subsection!.SectionId, ct)!;
        if (section!.UserId != request.UserId) throw new ForbiddenException();

        topic.RemoveLink(request.LinkId);
        await uow.SaveChangesAsync(ct);
    }
}
