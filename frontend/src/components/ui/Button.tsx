import type { ButtonHTMLAttributes, ReactNode } from 'react';

interface Props extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  children: ReactNode;
  fullWidth?: boolean;
  loading?: boolean;
}

const variants = {
  primary: 'bg-[var(--tg-theme-button-color,#2481cc)] text-[var(--tg-theme-button-text-color,#fff)] active:opacity-80',
  secondary: 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] text-[var(--tg-theme-text-color,#000)]',
  ghost: 'text-[var(--tg-theme-link-color,#2481cc)] hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]',
  danger: 'bg-red-500 text-white active:opacity-80',
};

const sizes = { sm: 'px-3 py-1.5 text-sm', md: 'px-4 py-2.5 text-base', lg: 'px-5 py-3 text-lg' };

export const Button = ({
  variant = 'primary', size = 'md', children, fullWidth, loading, className = '', ...props
}: Props) => (
  <button
    {...props}
    disabled={loading || props.disabled}
    className={`${variants[variant]} ${sizes[size]} rounded-xl font-medium transition-all duration-150 select-none disabled:opacity-50 disabled:cursor-not-allowed ${fullWidth ? 'w-full' : ''} ${className}`}
  >
    {loading ? <span className="inline-block animate-spin">⏳</span> : children}
  </button>
);
