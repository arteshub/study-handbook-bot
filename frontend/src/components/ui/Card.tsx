import type { ReactNode } from 'react';

interface Props { children: ReactNode; onClick?: () => void; className?: string; padded?: boolean; }

export const Card = ({ children, onClick, className = '', padded = true }: Props) => (
  <div
    onClick={onClick}
    className={`bg-[var(--tg-theme-secondary-bg-color,#f8f8f8)] rounded-2xl ${padded ? 'p-4' : ''} ${onClick ? 'cursor-pointer active:scale-[0.98] transition-transform duration-100' : ''} ${className}`}
  >
    {children}
  </div>
);
