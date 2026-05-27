using HandbookBot.Application.Interfaces;
using MediatR;

namespace HandbookBot.Application.YoutubeExtraction;

public class YoutubeExtractCommandHandler(IYoutubeExtractionService youtubeService)
    : IRequestHandler<YoutubeExtractCommand, YoutubeExtractResult>
{
    public Task<YoutubeExtractResult> Handle(YoutubeExtractCommand request, CancellationToken cancellationToken)
        => youtubeService.ExtractAndGenerateAsync(request.Url, null, cancellationToken);
}
