import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { Route, Routes } from 'react-router-dom';
import { CirclePlay } from 'lucide-react';
import { WebApp } from './lib/telegram';
import { setUserId } from './api/client';
import { BottomNav } from './components/BottomNav';
import { TreeSidebar } from './components/TreeSidebar';
import { GenerationProgressProvider, useGenerationProgress } from './context/GenerationProgressContext';
import { HistoryPage } from './pages/HistoryPage';
import { HomePage } from './pages/HomePage';
import { SectionPage } from './pages/SectionPage';
import { SettingsPage } from './pages/SettingsPage';
import { SubsectionPage } from './pages/SubsectionPage';
import { TestPage } from './pages/TestPage';
import { TopicEditorPage } from './pages/TopicEditorPage';
import { TopicPage } from './pages/TopicPage';

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, staleTime: 30_000 } },
});

const ActiveGenerationsBar = () => {
  const { jobs } = useGenerationProgress();
  const active = [...jobs.values()].filter(j => !j.isCompleted);
  if (!active.length) return null;

  return (
    <div className="fixed bottom-[60px] left-0 right-0 z-40 px-3 pb-1 flex flex-col gap-1.5 max-w-lg mx-auto">
      {active.map(job => (
        <div key={job.topicId} className="rounded-xl bg-[var(--tg-theme-bg-color,#fff)] shadow-lg border border-black/5 px-3 py-2">
          <div className="flex items-center gap-2 mb-1.5">
            <CirclePlay size={13} className="text-red-500 shrink-0" />
            <span className="text-xs font-medium truncate flex-1 opacity-80">{job.videoTitle}</span>
            <span className="text-xs font-semibold text-red-500 shrink-0">{job.progress}%</span>
          </div>
          <div className="w-full h-1 rounded-full bg-black/5 overflow-hidden">
            <div
              className="h-full rounded-full bg-red-500 transition-all duration-500"
              style={{ width: `${job.progress}%` }}
            />
          </div>
          {job.error && <p className="text-[10px] text-red-500 mt-1 truncate">{job.error}</p>}
        </div>
      ))}
    </div>
  );
};

export const App = () => {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    WebApp.ready();
    WebApp.expand();
    const userId = WebApp.initDataUnsafe?.user?.id;
    if (userId) setUserId(userId);
    else setUserId(12345);
  }, []);

  return (
    <QueryClientProvider client={queryClient}>
      <GenerationProgressProvider>
        <div className="max-w-lg mx-auto">
          <TreeSidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />

          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/sections/:id" element={<SectionPage />} />
            <Route path="/subsections/:id" element={<SubsectionPage />} />
            <Route path="/topics/:id" element={<TopicPage />} />
            <Route path="/topics/:id/edit" element={<TopicEditorPage />} />
            <Route path="/test" element={<TestPage />} />
            <Route path="/history" element={<HistoryPage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Routes>

          <ActiveGenerationsBar />
          <BottomNav onOpenTree={() => setSidebarOpen(true)} />
        </div>
      </GenerationProgressProvider>
    </QueryClientProvider>
  );
};
