import { useQuery } from '@tanstack/react-query';
import { testsApi } from '../api/tests';
import { Card } from '../components/ui/Card';
import { EmptyState } from '../components/ui/EmptyState';
import { Spinner } from '../components/ui/Spinner';
import type { TestSession } from '../types';

const SessionCard = ({ s }: { s: TestSession }) => {
  const pct = s.totalQuestions > 0 ? Math.round((s.correctAnswers / s.totalQuestions) * 100) : 0;
  return (
    <Card>
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm font-medium">{s.mode === 'AI' ? '🤖 AI-тест' : '🤔 Самооценка'}</span>
        <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${pct >= 80 ? 'bg-green-100 text-green-600' : pct >= 50 ? 'bg-yellow-100 text-yellow-600' : 'bg-red-100 text-red-500'}`}>{pct}%</span>
      </div>
      <div className="h-1.5 bg-[var(--tg-theme-bg-color,#fff)] rounded-full overflow-hidden mb-2">
        <div className="h-full rounded-full bg-[var(--tg-theme-button-color,#2481cc)]" style={{ width: `${pct}%` }} />
      </div>
      <div className="flex justify-between text-xs opacity-50">
        <span>{s.correctAnswers} / {s.totalQuestions} правильно</span>
        <span>{new Date(s.startedAt).toLocaleDateString('ru-RU')}</span>
      </div>
    </Card>
  );
};

export const HistoryPage = () => {
  const { data, isLoading } = useQuery({ queryKey: ['test-history'], queryFn: testsApi.getHistory });
  if (isLoading) return <Spinner />;
  return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <h1 className="text-2xl font-bold mb-2">История</h1>
      <p className="text-sm opacity-50 mb-6">Результаты тестов</p>
      {data?.length === 0
        ? <EmptyState icon="📊" title="Нет результатов" subtitle="Пройди первый тест!" />
        : <div className="flex flex-col gap-3">{data?.map(s => <SessionCard key={s.id} s={s} />)}</div>
      }
    </div>
  );
};
