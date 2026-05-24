import { BookOpen, FlaskConical, History, Map, Settings } from 'lucide-react';
import { useLocation, useNavigate } from 'react-router-dom';

interface Props {
  onOpenTree: () => void;
}

export const BottomNav = ({ onOpenTree }: Props) => {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  const tabs = [
    { label: 'Справочник', icon: BookOpen, onClick: () => navigate('/'), active: pathname === '/' },
    { label: 'Граф', icon: Map, onClick: onOpenTree, active: false },
    { label: 'Тест', icon: FlaskConical, onClick: () => navigate('/test'), active: pathname === '/test' },
    { label: 'История', icon: History, onClick: () => navigate('/history'), active: pathname === '/history' },
    { label: 'Настройки', icon: Settings, onClick: () => navigate('/settings'), active: pathname === '/settings' },
  ];

  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-[var(--tg-theme-bg-color,#fff)] border-t border-black/5 safe-bottom">
      <div className="flex">
        {tabs.map(({ label, icon: Icon, onClick, active }) => (
          <button
            key={label}
            onClick={onClick}
            className={`flex-1 flex flex-col items-center gap-1 py-2 transition-colors ${
              active
                ? 'text-[var(--tg-theme-button-color,#2481cc)]'
                : 'text-[var(--tg-theme-hint-color,#999)]'
            }`}
          >
            <Icon size={20} strokeWidth={active ? 2.5 : 1.8} />
            <span className="text-[9px] font-medium">{label}</span>
          </button>
        ))}
      </div>
    </nav>
  );
};
