using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.DeleteTopic;

internal sealed class DeleteTopicCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteTopicCommand>
{
    public async Task Handle(DeleteTopicCommand request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Topic), request.Id);

        var subsection = await uow.Subsections.GetByIdAsync(topic.SubsectionId, ct)!;
        var section = await uow.Sections.GetByIdAsync(subsection!.SectionId, ct)!;

        if (section!.UserId != request.UserId) throw new ForbiddenException();

        uow.Topics.Remove(topic);
        await uow.SaveChangesAsync(ct);
    }
}
