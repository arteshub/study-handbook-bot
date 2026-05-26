using HandbookBot.Domain.Enums;

namespace HandbookBot.Application.Tests.Dtos;

public sealed record TestSessionDto(
    Guid Id,
    TestMode Mode,
    int TotalQuestions,
    int CorrectAnswers,
    int AnsweredCount,
    bool IsCompleted,
    DateTime StartedAt,
    DateTime? CompletedAt);

public sealed record QuestionOptionDto(int Index, string Text);

public sealed record TestQuestionDto(
    Guid ResultId,
    Guid TopicId,
    string TopicTitle,
    string Question,
    string? CorrectAnswer,
    int QuestionNumber,
    int TotalQuestions,
    IReadOnlyList<QuestionOptionDto>? Options,
    Guid? CachedQuestionId = null);

public sealed record AnswerOptionResultDto(int Index, string Text, bool IsCorrect, string Explanation);

public sealed record TestAnswerResultDto(
    bool IsCorrect,
    string CorrectAnswer,
    string? AiFeedback,
    int CorrectAnswers,
    int TotalAnswered,
    IReadOnlyList<AnswerOptionResultDto>? OptionResults);
