import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { Route, Routes } from 'react-router-dom';
import { Map } from 'lucide-react';
import WebApp from '@twa-dev/sdk';
import { setUserId } from './api/client';
import { BottomNav } from './components/BottomNav';
import { TreeSidebar } from './components/TreeSidebar';
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
      <div className="max-w-lg mx-auto">
        <button
          onClick={() => setSidebarOpen(true)}
          className="fixed top-4 right-4 z-30 w-10 h-10 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] shadow-sm"
        >
          <Map size={18} className="opacity-60" />
        </button>

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
        <BottomNav />
      </div>
    </QueryClientProvider>
  );
};
