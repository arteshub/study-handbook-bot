import type { ReactNode } from 'react';

interface Props { open: boolean; title: string; onClose: () => void; children: ReactNode; actions?: ReactNode; }

export const Modal = ({ open, title, onClose, children, actions }: Props) => {
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 backdrop-blur-sm" onClick={onClose}>
      <div className="w-full max-w-lg bg-[var(--tg-theme-bg-color,#fff)] rounded-t-3xl p-6 pb-8" onClick={e => e.stopPropagation()}>
        <div className="w-10 h-1 bg-[var(--tg-theme-hint-color,#999)] rounded-full mx-auto mb-4 opacity-40" />
        <h2 className="text-lg font-bold mb-4">{title}</h2>
        {children}
        {actions && <div className="flex gap-2 mt-4">{actions}</div>}
      </div>
    </div>
  );
};
