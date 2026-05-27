using HandbookBot.Application.Interfaces;
using HandbookBot.Domain.Entities;
using HandbookBot.Domain.Repositories;
using MediatR;

namespace HandbookBot.Application.YoutubeExtraction;

public class StartYoutubeGenerationCommandHandler(
    IUnitOfWork uow,
    IYoutubeExtractionService youtubeService,
    IBackgroundGenerationService backgroundService)
    : IRequestHandler<StartYoutubeGenerationCommand, StartGenerationResult>
{
    public async Task<StartGenerationResult> Handle(StartYoutubeGenerationCommand request, CancellationToken cancellationToken)
    {
        var videoTitle = await youtubeService.GetVideoTitleAsync(request.Url, cancellationToken);

        var existing = await uow.Topics.FindAsync(t => t.SubsectionId == request.SubsectionId, cancellationToken);
        var topic = Topic.Create(request.SubsectionId, videoTitle, string.Empty, null, existing.Count);
        await uow.Topics.AddAsync(topic, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        if (!backgroundService.TryStartGeneration(request.UserId, topic.Id, request.Url, videoTitle))
        {
            uow.Topics.Remove(topic);
            await uow.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Максимум 2 одновременных генерации. Дождитесь завершения текущих.");
        }

        return new StartGenerationResult(topic.Id, videoTitle);
    }
}
