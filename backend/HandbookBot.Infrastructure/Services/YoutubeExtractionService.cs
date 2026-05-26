using System.ClientModel;
using System.Text;
using HandbookBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using YoutubeExplode;

namespace HandbookBot.Infrastructure.Services;

public class YoutubeExtractionService(YoutubeClient youtubeClient, IConfiguration config) : IYoutubeExtractionService
{
    private string ApiKey => config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

    public async Task<YoutubeExtractResult> ExtractAndGenerateAsync(string url, CancellationToken ct = default)
    {
        var video = await youtubeClient.Videos.GetAsync(url, ct);
        var videoTitle = video.Title;

        var manifest = await youtubeClient.Videos.ClosedCaptions.GetManifestAsync(url, ct);
        var trackInfo = manifest.Tracks
            .FirstOrDefault(t => t.Language.Code.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
            ?? manifest.Tracks.FirstOrDefault()
            ?? throw new InvalidOperationException("Видео не содержит субтитров.");

        var track = await youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo, ct);

        var sb = new StringBuilder();
        foreach (var caption in track.Captions)
        {
            var offset = caption.Offset;
            sb.AppendLine($"[{(int)offset.TotalMinutes:D2}:{offset.Seconds:D2}] {caption.Text}");
        }
        var subtitles = sb.ToString();

        var prompt =
            $"Ты — опытный технический писатель. Тебе дали субтитры технического доклада \"{videoTitle}\".\n\n" +
            $"ЗАДАЧА: написать полный технический справочник для разработчиков на основе ВСЕГО содержимого доклада.\n\n" +
            $"ОБЯЗАТЕЛЬНЫЕ ПРАВИЛА:\n" +
            $"1. Воспроизводи ВЕСЬ учебный материал без исключений — каждый концепт, каждое объяснение, каждый пример, каждую деталь\n" +
            $"2. Пропускай ТОЛЬКО: рекламные вставки, спонсорские блоки, призывы подписаться/поставить лайк, флуд не по теме\n" +
            $"3. Технические объяснения передавай полно и точно, не сокращай и не обобщай\n" +
            $"4. Если докладчик показывает код — обязательно воспроизводи его с поясняющими комментариями на русском\n\n" +
            $"ФОРМАТ СТАТЬИ:\n" +
            $"- Крупные тематические блоки: ## ЧАСТЬ I. НАЗВАНИЕ\n" +
            $"- Пронумерованные разделы: ### 1. Название раздела [ЧЧ:ММ – ЧЧ:ММ]\n" +
            $"- Каждый концепт: что это, зачем нужно, как работает под капотом\n" +
            $"- Код в блоках с указанием языка, комментарии на русском\n" +
            $"- Неформальный, но точный технический язык — как у хорошего лектора\n" +
            $"- Сравнения с другими языками/подходами там, где докладчик упоминает\n" +
            $"- Выделяй важные советы, антипаттерны, ловушки\n" +
            $"- Последний раздел: ## ШПАРГАЛКА ДЛЯ СОБЕСА — таблицы, правила, быстрые ответы\n" +
            $"- Только Markdown, весь текст на русском\n\n" +
            $"Субтитры (с тайм-кодами):\n{subtitles}";

        var chatClient = new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(ApiKey),
            new OpenAIClientOptions { NetworkTimeout = TimeSpan.FromMinutes(10) });

        var response = await chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            new ChatCompletionOptions { MaxOutputTokenCount = 16000 },
            ct);

        var content = response.Value.Content[0].Text;
        return new YoutubeExtractResult(videoTitle, content);
    }
}
