using MediatR;

namespace HandbookBot.Application.YoutubeExtraction;

public record StartYoutubeGenerationCommand(string Url, Guid SubsectionId, long UserId) : IRequest<Guid>;
