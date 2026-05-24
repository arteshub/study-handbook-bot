import { Component, type ReactNode } from 'react';

interface Props { children: ReactNode; }
interface State { error: Error | null; }

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  render() {
    if (this.state.error) {
      return (
        <div className="min-h-screen flex flex-col items-center justify-center px-6 text-center">
          <div className="text-4xl mb-4">⚠️</div>
          <h1 className="text-lg font-bold mb-2">Что-то пошло не так</h1>
          <p className="text-sm opacity-50 mb-6">{this.state.error.message}</p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 rounded-xl bg-[var(--tg-theme-button-color,#2481cc)] text-white text-sm font-medium"
          >
            Перезагрузить
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
