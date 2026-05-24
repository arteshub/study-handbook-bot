using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.UpdateTopic;

internal sealed class UpdateTopicCommandHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateTopicCommand>
{
    public async Task Handle(UpdateTopicCommand request, CancellationToken ct)
    {
        var topic = await uow.Topics.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(Topic), request.Id);

        var subsection = await uow.Subsections.GetByIdAsync(topic.SubsectionId, ct)!;
        var section = await uow.Sections.GetByIdAsync(subsection!.SectionId, ct)!;

        if (section!.UserId != request.UserId) throw new ForbiddenException();

        topic.Update(request.Title, request.Content, request.Summary);
        uow.Topics.Update(topic);
        await uow.SaveChangesAsync(ct);
    }
}
