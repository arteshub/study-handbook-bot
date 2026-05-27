using MediatR;

namespace HandbookBot.Application.YoutubeExtraction;

public record StartGenerationResult(Guid TopicId, string VideoTitle);

public record StartYoutubeGenerationCommand(string Url, Guid SubsectionId, long UserId) : IRequest<StartGenerationResult>;
