import { BookOpen, FlaskConical, History, Settings } from 'lucide-react';
import { useLocation, useNavigate } from 'react-router-dom';

const tabs = [
  { path: '/', icon: BookOpen, label: 'Справочник' },
  { path: '/test', icon: FlaskConical, label: 'Тест' },
  { path: '/history', icon: History, label: 'История' },
  { path: '/settings', icon: Settings, label: 'Настройки' },
];

export const BottomNav = () => {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-[var(--tg-theme-bg-color,#fff)] border-t border-black/5 safe-bottom">
      <div className="flex">
        {tabs.map(({ path, icon: Icon, label }) => {
          const active = pathname === path || (path !== '/' && pathname.startsWith(path));
          return (
            <button
              key={path}
              onClick={() => navigate(path)}
              className={`flex-1 flex flex-col items-center gap-1 py-2 transition-colors ${
                active
                  ? 'text-[var(--tg-theme-button-color,#2481cc)]'
                  : 'text-[var(--tg-theme-hint-color,#999)]'
              }`}
            >
              <Icon size={22} strokeWidth={active ? 2.5 : 1.8} />
              <span className="text-[10px] font-medium">{label}</span>
            </button>
          );
        })}
      </div>
    </nav>
  );
};
