import type { InputHTMLAttributes } from 'react';

interface Props extends InputHTMLAttributes<HTMLInputElement> { label?: string; }

export const Input = ({ label, className = '', ...props }: Props) => (
  <div className="flex flex-col gap-1">
    {label && <label className="text-sm font-medium opacity-70">{label}</label>}
    <input {...props} className={`w-full px-4 py-3 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] text-[var(--tg-theme-text-color,#000)] border border-transparent focus:border-[var(--tg-theme-button-color,#2481cc)] outline-none transition-colors text-base placeholder:text-[var(--tg-theme-hint-color,#999)] ${className}`} />
  </div>
);
