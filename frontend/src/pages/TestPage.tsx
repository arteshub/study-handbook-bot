import { useQuery, useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { sectionsApi } from '../api/sections';
import { testsApi } from '../api/tests';
import type { TestMode, TestQuestion, TestSession, TestAnswerResult } from '../types';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { Spinner } from '../components/ui/Spinner';

type Step = 'setup' | 'question' | 'reveal' | 'result';

export const TestPage = () => {
  const [step, setStep] = useState<Step>('setup');
  const [mode, setMode] = useState<TestMode>('Self');
  const [sectionId, setSectionId] = useState('');
  const [session, setSession] = useState<TestSession | null>(null);
  const [question, setQuestion] = useState<TestQuestion | null>(null);
  const [answer, setAnswer] = useState('');
  const [answerResult, setAnswerResult] = useState<TestAnswerResult | null>(null);
  const [showAnswer, setShowAnswer] = useState(false);

  const { data: sections } = useQuery({ queryKey: ['sections'], queryFn: sectionsApi.getAll });

  const start = useMutation({
    mutationFn: () => testsApi.startSession({ mode, sectionId: sectionId || undefined }),
    onSuccess: async (s) => {
      setSession(s);
      const q = await testsApi.getNextQuestion(s.id);
      setQuestion(q);
      setStep('question');
    },
  });

  const submit = useMutation({
    mutationFn: (selfCorrect: boolean | undefined) => {
      if (!session || !question) throw new Error();
      return testsApi.submitAnswer(session.id, {
        resultId: question.resultId,
        userAnswer: mode === 'AI' ? answer : undefined,
        selfMarkedCorrect: selfCorrect,
      });
    },
    onSuccess: (res) => { setAnswerResult(res); setStep('reveal'); },
  });

  const nextQuestion = async () => {
    if (!session) return;
    const q = await testsApi.getNextQuestion(session.id);
    if (!q) {
      const completed = await testsApi.complete(session.id);
      setSession(completed);
      setStep('result');
    } else {
      setQuestion(q);
      setAnswer('');
      setShowAnswer(false);
      setAnswerResult(null);
      setStep('question');
    }
  };

  if (step === 'setup') return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <h1 className="text-2xl font-bold mb-2">Тест</h1>
      <p className="text-sm opacity-50 mb-6">Проверь свои знания</p>
      <div className="flex flex-col gap-4">
        <Card>
          <p className="text-sm font-medium opacity-60 mb-2">Режим теста</p>
          <div className="grid grid-cols-2 gap-2">
            {(['Self', 'AI'] as TestMode[]).map(m => (
              <button key={m} onClick={() => setMode(m)}
                className={`py-3 rounded-xl text-sm font-medium transition-colors ${mode === m ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white' : 'bg-[var(--tg-theme-bg-color,#fff)]'}`}>
                {m === 'Self' ? '🤔 Самооценка' : '🤖 AI-оценка'}
              </button>
            ))}
          </div>
          <p className="text-xs opacity-40 mt-2">{mode === 'Self' ? 'Ты сам решаешь, знал ли ответ' : 'ChatGPT оценивает твой ответ'}</p>
        </Card>
        <Card>
          <p className="text-sm font-medium opacity-60 mb-2">Раздел</p>
          <select value={sectionId} onChange={e => setSectionId(e.target.value)}
            className="w-full bg-[var(--tg-theme-bg-color,#fff)] rounded-xl p-3 text-sm outline-none">
            <option value="">Все разделы</option>
            {sections?.map(s => <option key={s.id} value={s.id}>{s.icon} {s.title}</option>)}
          </select>
        </Card>
        <Button fullWidth size="lg" loading={start.isPending} onClick={() => start.mutate()}>Начать тест</Button>
      </div>
    </div>
  );

  if (step === 'question' && question) return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <div className="flex items-center justify-between mb-4">
        <span className="text-sm opacity-50">Вопрос {question.questionNumber} / {question.totalQuestions}</span>
        <div className="h-2 flex-1 mx-4 bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-full overflow-hidden">
          <div className="h-full bg-[var(--tg-theme-button-color,#2481cc)] rounded-full" style={{ width: `${(question.questionNumber / question.totalQuestions) * 100}%` }} />
        </div>
      </div>
      <Card className="mb-4">
        <p className="text-xs opacity-40 mb-1">📝 {question.topicTitle}</p>
        <p className="text-lg font-semibold leading-snug">{question.question}</p>
      </Card>
      {mode === 'AI' ? (
        <>
          <textarea value={answer} onChange={e => setAnswer(e.target.value)} placeholder="Напиши ответ..."
            className="w-full p-4 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] text-base outline-none resize-none mb-4" rows={4} />
          <Button fullWidth loading={submit.isPending} onClick={() => submit.mutate(undefined)}>Ответить</Button>
        </>
      ) : (
        !showAnswer
          ? <Button fullWidth variant="secondary" onClick={() => setShowAnswer(true)}>Показать ответ</Button>
          : <>
              <Card className="mb-4 border-l-4 border-[var(--tg-theme-button-color,#2481cc)]">
                <p className="text-sm opacity-60 mb-1">Ответ из справочника:</p>
                <p className="text-sm leading-relaxed">{question.question}</p>
              </Card>
              <div className="flex gap-3">
                <Button fullWidth variant="danger" onClick={() => submit.mutate(false)}>❌ Не знал</Button>
                <Button fullWidth onClick={() => submit.mutate(true)}>✅ Знал</Button>
              </div>
            </>
      )}
    </div>
  );

  if (step === 'reveal' && answerResult) return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <div className={`p-4 rounded-2xl mb-4 ${answerResult.isCorrect ? 'bg-green-50' : 'bg-red-50'}`}>
        <p className="text-2xl mb-1">{answerResult.isCorrect ? '✅' : '❌'}</p>
        <p className="font-semibold">{answerResult.isCorrect ? 'Правильно!' : 'Не совсем...'}</p>
        {answerResult.aiFeedback && <p className="text-sm opacity-70 mt-1">{answerResult.aiFeedback}</p>}
      </div>
      <Card className="mb-4">
        <p className="text-sm opacity-60 mb-1">Правильный ответ:</p>
        <p className="text-sm leading-relaxed line-clamp-8">{answerResult.correctAnswer}</p>
      </Card>
      <p className="text-center text-sm opacity-50 mb-4">{answerResult.correctAnswers} / {answerResult.totalAnswered} правильно</p>
      <Button fullWidth onClick={nextQuestion}>Следующий вопрос →</Button>
    </div>
  );

  if (step === 'result' && session) return (
    <div className="px-4 pt-12 pb-24 min-h-screen flex flex-col items-center text-center">
      <div className="text-6xl mb-4">{session.correctAnswers / session.totalQuestions >= 0.8 ? '🏆' : '📚'}</div>
      <h2 className="text-2xl font-bold mb-2">Тест завершён!</h2>
      <p className="text-4xl font-bold text-[var(--tg-theme-button-color,#2481cc)] mb-1">{session.correctAnswers} / {session.totalQuestions}</p>
      <p className="text-sm opacity-50 mb-8">{Math.round((session.correctAnswers / session.totalQuestions) * 100)}% правильных ответов</p>
      <Button fullWidth onClick={() => { setStep('setup'); setSession(null); setQuestion(null); }}>Пройти ещё раз</Button>
    </div>
  );

  return <Spinner />;
};
