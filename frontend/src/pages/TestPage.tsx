import { useQuery, useMutation, useQueries } from '@tanstack/react-query';
import MDEditor from '@uiw/react-md-editor';
import { BookOpen, SkipForward } from 'lucide-react';
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { sectionsApi } from '../api/sections';
import { subsectionsApi } from '../api/subsections';
import { topicsApi } from '../api/topics';
import { testsApi } from '../api/tests';
import type { TestMode, TestQuestion, TestSession, TestAnswerResult } from '../types';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { Spinner } from '../components/ui/Spinner';
import { TopicChat } from '../components/TopicChat';

type Step = 'setup' | 'question' | 'reveal' | 'result';

const LETTERS = ['А', 'Б', 'В', 'Г'];

export const TestPage = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState<Step>('setup');
  const [mode, setMode] = useState<TestMode>('Self');
  const [selectedSectionIds, setSelectedSectionIds] = useState<string[]>([]);
  const [selectedSubsectionIds, setSelectedSubsectionIds] = useState<string[]>([]);
  const [selectedTopicIds, setSelectedTopicIds] = useState<string[]>([]);
  const [isReviewMode, setIsReviewMode] = useState(false);
  const [session, setSession] = useState<TestSession | null>(null);
  const [question, setQuestion] = useState<TestQuestion | null>(null);
  const [answerResult, setAnswerResult] = useState<TestAnswerResult | null>(null);
  const [userAnswer, setUserAnswer] = useState('');
  const [showAnswer, setShowAnswer] = useState(false);
  const [selectedOption, setSelectedOption] = useState<number | null>(null);
  const [startError, setStartError] = useState('');
  const [skipping, setSkipping] = useState(false);
  const [showChat, setShowChat] = useState(false);

  const { data: reviewStats } = useQuery({
    queryKey: ['review-stats'],
    queryFn: testsApi.getReviewStats,
    refetchInterval: 60_000,
  });

  const { data: sections } = useQuery({ queryKey: ['sections'], queryFn: sectionsApi.getAll });

  const subsectionResults = useQueries({
    queries: selectedSectionIds.map(id => ({
      queryKey: ['subsections', id],
      queryFn: () => subsectionsApi.getBySectionId(id),
    })),
  });
  const allSubsections = subsectionResults.flatMap(r => r.data ?? []);

  const topicResults = useQueries({
    queries: selectedSubsectionIds.map(id => ({
      queryKey: ['topics', id],
      queryFn: () => topicsApi.getBySubsectionId(id),
    })),
  });
  const allTopics = topicResults.flatMap(r => r.data ?? []);

  const toggleSection = (id: string) => {
    setSelectedSectionIds(prev => {
      const next = prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id];
      if (!next.includes(id)) {
        setSelectedSubsectionIds(s => s.filter(sid => allSubsections.find(ss => ss.id === sid)?.sectionId !== id));
        setSelectedTopicIds([]);
      }
      return next;
    });
  };
  const toggleSubsection = (id: string) => {
    setSelectedSubsectionIds(prev => {
      const next = prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id];
      if (!next.includes(id)) setSelectedTopicIds(t => t.filter(tid => allTopics.find(tp => tp.id === tid)?.subsectionId !== id));
      return next;
    });
  };
  const toggleTopic = (id: string) =>
    setSelectedTopicIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);

  useEffect(() => {
    const raw = sessionStorage.getItem('activeTest');
    if (!raw) return;
    try {
      const { id, isReview } = JSON.parse(raw) as { id: string; isReview: boolean };
      testsApi.getSession(id).then(s => {
        if (!s.isCompleted) {
          setSession(s);
          setIsReviewMode(isReview);
          testsApi.getNextQuestion(s.id).then(q => {
            if (!q) {
              sessionStorage.removeItem('activeTest');
            } else {
              setQuestion(q);
              setStep('question');
            }
          });
        } else {
          sessionStorage.removeItem('activeTest');
        }
      }).catch(() => sessionStorage.removeItem('activeTest'));
    } catch {
      sessionStorage.removeItem('activeTest');
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const loadQuestion = async (s: TestSession) => {
    const q = await testsApi.getNextQuestion(s.id);
    if (!q) {
      const completed = await testsApi.complete(s.id);
      sessionStorage.removeItem('activeTest');
      setSession(completed);
      setStep('result');
    } else {
      setQuestion(q);
      setAnswerResult(null);
      setUserAnswer('');
      setShowAnswer(false);
      setShowChat(false);
      setSelectedOption(null);
      setStep('question');
    }
  };

  const start = useMutation({
    mutationFn: (reviewMode = false) => testsApi.startSession({
      mode: reviewMode ? 'Self' : mode,
      sectionIds: reviewMode || selectedSectionIds.length === 0 ? undefined : selectedSectionIds,
      subsectionIds: reviewMode || selectedSubsectionIds.length === 0 ? undefined : selectedSubsectionIds,
      topicIds: reviewMode || selectedTopicIds.length === 0 ? undefined : selectedTopicIds,
      reviewMode,
    }),
    onSuccess: async (s, reviewMode) => {
      sessionStorage.setItem('activeTest', JSON.stringify({ id: s.id, isReview: !!reviewMode }));
      setSession(s);
      setIsReviewMode(!!reviewMode);
      setStartError('');
      if (s.totalQuestions === 0) {
        setStartError('Нет тем с контентом в выбранной области. Добавь материал в темы.');
        sessionStorage.removeItem('activeTest');
        return;
      }
      await loadQuestion(s);
    },
    onError: (e: any) => {
      setStartError(e?.response?.data?.detail ?? e?.message ?? 'Ошибка при создании теста');
    },
  });

  const submit = useMutation({
    mutationFn: (args: { selfCorrect?: boolean; optionIndex?: number }) =>
      testsApi.submitAnswer(session!.id, {
        resultId: question!.resultId,
        selfMarkedCorrect: args.selfCorrect,
        selectedOptionIndex: args.optionIndex,
      }),
    onSuccess: (res, args) => {
      setAnswerResult(res);
      if (args.optionIndex === undefined) {
        // Self mode — skip reveal, go to next question
        loadQuestion(session!);
      } else {
        setStep('reveal');
      }
    },
  });

  const skipTopic = async () => {
    if (!session || !question) return;
    setSkipping(true);
    try {
      await testsApi.skipTopic(session.id, question.topicId);
      await loadQuestion(session);
    } finally {
      setSkipping(false);
    }
  };

  const nextQuestion = async () => {
    if (!session) return;
    await loadQuestion(session);
  };

  const restart = () => {
    sessionStorage.removeItem('activeTest');
    setStep('setup');
    setSession(null);
    setQuestion(null);
    setAnswerResult(null);
    setUserAnswer('');
    setShowAnswer(false);
    setShowChat(false);
    setSelectedOption(null);
    setStartError('');
    setIsReviewMode(false);
    setSelectedSectionIds([]);
    setSelectedSubsectionIds([]);
    setSelectedTopicIds([]);
  };

  const scopeLabel = selectedTopicIds.length > 0
    ? `${selectedTopicIds.length} ${selectedTopicIds.length === 1 ? 'тема' : 'тем'}`
    : selectedSubsectionIds.length > 0
    ? `${selectedSubsectionIds.length} ${selectedSubsectionIds.length === 1 ? 'подраздел' : 'подразделов'}`
    : selectedSectionIds.length === 1
    ? `${sections?.find(s => s.id === selectedSectionIds[0])?.icon ?? ''} ${sections?.find(s => s.id === selectedSectionIds[0])?.title ?? ''}`
    : selectedSectionIds.length > 1
    ? `${selectedSectionIds.length} разделов`
    : 'Все разделы';

  // ── Setup ──────────────────────────────────────────────────────────────────
  if (step === 'setup') return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <h1 className="text-2xl font-bold mb-2">Тест</h1>
      <p className="text-sm opacity-50 mb-6">Проверь свои знания</p>
      <div className="flex flex-col gap-4">

        {(reviewStats?.dueCount ?? 0) > 0 && (
          <button
            onClick={() => start.mutate(true)}
            disabled={start.isPending}
            className="flex items-center gap-3 p-4 rounded-2xl bg-[var(--tg-theme-button-color,#2481cc)] text-white text-left"
          >
            <span className="text-2xl">🔁</span>
            <div className="flex-1">
              <p className="font-bold text-base">Повторить сегодня</p>
              <p className="text-sm opacity-80">{reviewStats!.dueCount} {reviewStats!.dueCount === 1 ? 'тема ждёт' : 'темы ждут'} повторения</p>
            </div>
            <span className="text-2xl font-bold opacity-70">→</span>
          </button>
        )}

        {(reviewStats?.dueCount ?? 0) === 0 && reviewStats !== undefined && (
          <div className="flex items-center gap-3 p-4 rounded-2xl bg-green-50 border border-green-100">
            <span className="text-2xl">✅</span>
            <div>
              <p className="font-semibold text-green-700">Всё повторено!</p>
              <p className="text-sm text-green-600 opacity-80">Новые повторения появятся позже</p>
            </div>
          </div>
        )}

        <Card>
          <p className="text-sm font-medium opacity-60 mb-2">Режим</p>
          <div className="grid grid-cols-2 gap-2">
            {([
              ['Self', '🤔 Самооценка', 'Вопросы глубже и глубже по теме'],
              ['AI', '🤖 Варианты', '4 варианта ответа + объяснения'],
            ] as [TestMode, string, string][]).map(([m, label, hint]) => (
              <button key={m} onClick={() => setMode(m)}
                className={`p-3 rounded-xl text-sm font-medium transition-colors text-left ${mode === m ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white' : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'}`}>
                <div>{label}</div>
                <div className={`text-xs mt-0.5 ${mode === m ? 'opacity-70' : 'opacity-40'}`}>{hint}</div>
              </button>
            ))}
          </div>
        </Card>

        <Card>
          <p className="text-sm font-medium opacity-60 mb-3">Область</p>

          {/* Section chips */}
          <p className="text-xs opacity-40 mb-2 px-0.5">Разделы</p>
          <div className="flex flex-wrap gap-2 mb-1">
            {sections?.map(s => (
              <button
                key={s.id}
                onClick={() => toggleSection(s.id)}
                className={`px-3 py-1.5 rounded-xl text-sm font-medium transition-colors ${
                  selectedSectionIds.includes(s.id)
                    ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white'
                    : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'
                }`}
              >
                {s.icon} {s.title}
              </button>
            ))}
          </div>

          {/* Subsection chips */}
          {selectedSectionIds.length > 0 && allSubsections.length > 0 && (
            <div className="mt-3">
              <p className="text-xs opacity-40 mb-2 px-0.5">Подразделы</p>
              <div className="flex flex-wrap gap-2 mb-1">
                {allSubsections.map(s => (
                  <button
                    key={s.id}
                    onClick={() => toggleSubsection(s.id)}
                    className={`px-3 py-1.5 rounded-xl text-sm font-medium transition-colors ${
                      selectedSubsectionIds.includes(s.id)
                        ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white'
                        : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'
                    }`}
                  >
                    {s.title}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* Topic chips */}
          {selectedSubsectionIds.length > 0 && allTopics.length > 0 && (
            <div className="mt-3">
              <p className="text-xs opacity-40 mb-2 px-0.5">Темы</p>
              <div className="flex flex-wrap gap-2">
                {allTopics.map(t => (
                  <button
                    key={t.id}
                    onClick={() => toggleTopic(t.id)}
                    className={`px-3 py-1.5 rounded-xl text-sm font-medium transition-colors ${
                      selectedTopicIds.includes(t.id)
                        ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white'
                        : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'
                    }`}
                  >
                    {t.title}
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="mt-3 p-2 rounded-lg bg-[var(--tg-theme-button-color,#2481cc)]/10">
            <p className="text-xs text-[var(--tg-theme-link-color,#2481cc)]">
              Выбрано: <span className="font-medium">{scopeLabel}</span>
            </p>
          </div>
        </Card>

        {startError && <div className="p-3 rounded-xl bg-red-50 text-sm text-red-500">{startError}</div>}
        <Button fullWidth size="lg" loading={start.isPending} onClick={() => start.mutate(false)}>Начать тест</Button>
      </div>
    </div>
  );

  // ── Question ───────────────────────────────────────────────────────────────
  if (step === 'question' && question) {
    const isMultiChoice = !!question.options?.length;
    return (
      <>
      <div className="px-4 pt-6 pb-24 min-h-screen">
        {/* Top bar: exit + progress */}
        <div className="flex items-center gap-3 mb-4">
          <button onClick={restart} className="w-7 h-7 flex items-center justify-center rounded-lg bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] shrink-0 text-sm">✕</button>
          <span className="text-sm opacity-50 shrink-0">{question.questionNumber} / {question.totalQuestions}</span>
          <div className="h-2 flex-1 bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-full overflow-hidden">
            <div className="h-full bg-[var(--tg-theme-button-color,#2481cc)] rounded-full transition-all"
              style={{ width: `${(question.questionNumber / question.totalQuestions) * 100}%` }} />
          </div>
          {/* Skip topic button */}
          <button
            onClick={skipTopic}
            disabled={skipping}
            title="Следующая тема"
            className="flex items-center gap-1 text-xs opacity-40 hover:opacity-70 shrink-0"
          >
            <SkipForward size={14} />
          </button>
        </div>

        {/* Topic link */}
        <button
          onClick={() => navigate(`/topics/${question.topicId}`)}
          className="flex items-center gap-1.5 text-xs text-[var(--tg-theme-link-color,#2481cc)] opacity-70 mb-3 hover:opacity-100"
        >
          <BookOpen size={11} />
          {question.topicTitle}
        </button>

        {/* Question */}
        <Card className="mb-4">
          <p className="text-base font-semibold leading-snug">{question.question}</p>
        </Card>

        {isMultiChoice ? (
          <div className="flex flex-col gap-2">
            {question.options!.map((opt, i) => (
              <button
                key={opt.index}
                onClick={() => {
                  if (selectedOption !== null) return;
                  setSelectedOption(opt.index);
                  submit.mutate({ optionIndex: opt.index });
                }}
                disabled={selectedOption !== null || submit.isPending}
                className={`flex items-center gap-3 p-3 rounded-xl text-left text-sm transition-colors border-2 ${
                  selectedOption === opt.index
                    ? 'border-[var(--tg-theme-button-color,#2481cc)] bg-[var(--tg-theme-button-color,#2481cc)]/10'
                    : 'border-transparent bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] hover:border-[var(--tg-theme-button-color,#2481cc)]/30'
                }`}
              >
                <span className="w-6 h-6 rounded-full bg-[var(--tg-theme-button-color,#2481cc)]/20 text-[var(--tg-theme-button-color,#2481cc)] text-xs font-bold flex items-center justify-center shrink-0">
                  {LETTERS[i]}
                </span>
                <span className="flex-1">{opt.text}</span>
              </button>
            ))}
            {submit.isPending && <div className="flex justify-center pt-2"><Spinner /></div>}
          </div>
        ) : (
          /* Self mode — think in your head, then reveal */
          <>
            {!showAnswer
              ? <Button fullWidth variant="secondary" onClick={() => setShowAnswer(true)}>Показать ответ</Button>
              : (
                <>
                  <div className="mb-4 p-3 rounded-2xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
                    <p className="text-xs opacity-50 mb-2">Эталонный ответ:</p>
                    <div data-color-mode="light" className="wmde-markdown-var">
                      <MDEditor.Markdown source={question.correctAnswer ?? ''} style={{ background: 'transparent', fontSize: 13, lineHeight: 1.6 }} />
                    </div>
                  </div>
                  <div className="flex gap-3 mb-3">
                    <Button fullWidth variant="danger" loading={submit.isPending} onClick={() => submit.mutate({ selfCorrect: false })}>❌ Не знал</Button>
                    <Button fullWidth loading={submit.isPending} onClick={() => submit.mutate({ selfCorrect: true })}>✅ Знал</Button>
                  </div>
                  <div className="flex gap-2 mb-0">
                    <button
                      onClick={() => setShowChat(true)}
                      className="flex-1 py-2.5 rounded-xl text-sm text-[var(--tg-theme-link-color,#2481cc)] bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] hover:opacity-80 transition-opacity"
                    >
                      💬 Спросить AI
                    </button>
                    <button
                      onClick={skipTopic}
                      disabled={skipping}
                      className="flex-1 py-2.5 rounded-xl text-sm opacity-50 bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] hover:opacity-70 transition-opacity flex items-center justify-center gap-1.5 disabled:opacity-30"
                    >
                      <SkipForward size={14} />
                      Следующая тема
                    </button>
                  </div>
                </>
              )
            }
          </>
        )}
      </div>

      {showChat && (
        <TopicChat
          topicId={question.topicId}
          questionContext={question.question}
          modelAnswerContext={question.correctAnswer ?? ''}
          onClose={() => setShowChat(false)}
        />
      )}
      </>
    );
  }

  // ── Reveal (AI mode only) ──────────────────────────────────────────────────
  if (step === 'reveal' && answerResult && question) {
    return (
      <>
      <div className="px-4 pt-6 pb-24 min-h-screen">
        <button onClick={restart} className="flex items-center gap-1.5 text-sm opacity-50 mb-4 hover:opacity-80">✕ Завершить тест</button>

        <div className={`p-4 rounded-2xl mb-4 ${answerResult.isCorrect ? 'bg-green-50' : 'bg-red-50'}`}>
          <p className="text-2xl mb-1">{answerResult.isCorrect ? '✅' : '❌'}</p>
          <p className="font-semibold">{answerResult.isCorrect ? 'Правильно!' : 'Неверно'}</p>
          <p className="text-xs opacity-50 mt-1">{answerResult.correctAnswers} / {answerResult.totalAnswered} правильно</p>
        </div>

        <button onClick={() => navigate(`/topics/${question.topicId}`)}
          className="flex items-center gap-1.5 text-xs text-[var(--tg-theme-link-color,#2481cc)] opacity-70 mb-3 hover:opacity-100">
          <BookOpen size={11} />
          {question.topicTitle} — открыть тему
        </button>

        <div className="flex flex-col gap-2 mb-4">
          {answerResult.optionResults!.map((opt, i) => (
            <div key={opt.index}
              className={`p-3 rounded-xl border-2 ${opt.isCorrect ? 'border-green-400 bg-green-50' : selectedOption === opt.index ? 'border-red-400 bg-red-50' : 'border-transparent bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'}`}>
              <div className="flex items-center gap-2 mb-1">
                <span className="text-sm font-bold">{LETTERS[i]}.</span>
                <span className="text-sm font-medium flex-1">{opt.text}</span>
                {opt.isCorrect ? <span className="text-green-500 text-sm">✓</span> : selectedOption === opt.index ? <span className="text-red-400 text-sm">✗</span> : null}
              </div>
              <p className="text-xs opacity-60 leading-relaxed ml-5">{opt.explanation}</p>
            </div>
          ))}
        </div>

        <div className="flex gap-3 mb-3">
          <Button fullWidth variant="secondary" onClick={skipTopic} loading={skipping}>
            <SkipForward size={14} className="mr-1 inline" />
            Следующая тема
          </Button>
          <Button fullWidth onClick={nextQuestion}>Следующий →</Button>
        </div>
        <button
          onClick={() => setShowChat(true)}
          className="w-full py-2.5 rounded-xl text-sm text-[var(--tg-theme-link-color,#2481cc)] bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] hover:opacity-80 transition-opacity"
        >
          💬 Спросить AI
        </button>
      </div>

      {showChat && (
        <TopicChat
          topicId={question.topicId}
          questionContext={question.question}
          modelAnswerContext={answerResult.correctAnswer}
          onClose={() => setShowChat(false)}
        />
      )}
      </>
    );
  }

  // ── Result ─────────────────────────────────────────────────────────────────
  if (step === 'result' && session) {
    const pct = session.totalQuestions > 0
      ? Math.round((session.correctAnswers / session.totalQuestions) * 100)
      : 0;
    return (
      <div className="px-4 pt-6 pb-24 min-h-screen flex flex-col items-center">
        <div className="self-start mb-6">
          <button onClick={restart} className="flex items-center gap-1.5 text-sm opacity-50 hover:opacity-80">✕ Закрыть</button>
        </div>
        <div className="flex flex-col items-center text-center flex-1">
          <div className="text-6xl mb-4">{pct >= 80 ? '🏆' : pct >= 50 ? '📚' : '💪'}</div>
          <h2 className="text-2xl font-bold mb-2">Тест завершён!</h2>
          <p className="text-4xl font-bold text-[var(--tg-theme-button-color,#2481cc)] mb-1">{session.correctAnswers} / {session.totalQuestions}</p>
          <p className="text-sm opacity-50 mb-8">{pct}% правильных ответов</p>
          <Button fullWidth onClick={restart}>Пройти ещё раз</Button>
        </div>
      </div>
    );
  }

  return <Spinner />;
};
