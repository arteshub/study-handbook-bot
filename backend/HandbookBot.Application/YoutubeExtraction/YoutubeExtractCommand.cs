using HandbookBot.Application.Interfaces;
using MediatR;

namespace HandbookBot.Application.YoutubeExtraction;

public record YoutubeExtractCommand(string Url) : IRequest<YoutubeExtractResult>;
