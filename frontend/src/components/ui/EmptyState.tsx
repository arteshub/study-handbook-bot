interface Props {
  icon?: string;
  title: string;
  subtitle?: string;
  action?: { label: string; onClick: () => void };
}

export const EmptyState = ({ icon = '📭', title, subtitle, action }: Props) => (
  <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
    <span className="text-5xl mb-4">{icon}</span>
    <h3 className="text-lg font-semibold mb-1">{title}</h3>
    {subtitle && <p className="text-sm opacity-60 mb-4">{subtitle}</p>}
    {action && (
      <button
        onClick={action.onClick}
        className="text-[var(--tg-theme-link-color,#2481cc)] font-medium text-sm"
      >
        {action.label}
      </button>
    )}
  </div>
);
