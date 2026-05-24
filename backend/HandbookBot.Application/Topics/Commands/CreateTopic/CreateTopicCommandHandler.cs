using HandbookBot.Application.Common.Exceptions;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.Topics.Commands.CreateTopic;

internal sealed class CreateTopicCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateTopicCommand, Guid>
{
    public async Task<Guid> Handle(CreateTopicCommand request, CancellationToken ct)
    {
        var subsection = await uow.Subsections.GetByIdAsync(request.SubsectionId, ct)
            ?? throw new NotFoundException(nameof(Subsection), request.SubsectionId);

        var section = await uow.Sections.GetByIdAsync(subsection.SectionId, ct)
            ?? throw new NotFoundException(nameof(Section), subsection.SectionId);

        if (section.UserId != request.UserId) throw new ForbiddenException();

        var existing = await uow.Topics.GetBySubsectionIdAsync(request.SubsectionId, ct);
        var topic = Topic.Create(request.SubsectionId, request.Title, request.Content, request.Summary, existing.Count);

        await uow.Topics.AddAsync(topic, ct);
        await uow.SaveChangesAsync(ct);

        return topic.Id;
    }
}
