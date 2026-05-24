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

public sealed record TestQuestionDto(
    Guid ResultId,
    Guid TopicId,
    string TopicTitle,
    string Question,
    int QuestionNumber,
    int TotalQuestions);

public sealed record TestAnswerResultDto(
    bool IsCorrect,
    string CorrectAnswer,
    string? AiFeedback,
    int CorrectAnswers,
    int TotalAnswered);
