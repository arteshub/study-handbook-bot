export const Spinner = ({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' }) => {
  const s = { sm: 'w-4 h-4', md: 'w-8 h-8', lg: 'w-12 h-12' }[size];
  return (
    <div className="flex justify-center items-center py-8">
      <div className={`${s} border-4 border-[var(--tg-theme-secondary-bg-color,#f1f1f1)] border-t-[var(--tg-theme-button-color,#2481cc)] rounded-full animate-spin`} />
    </div>
  );
};
