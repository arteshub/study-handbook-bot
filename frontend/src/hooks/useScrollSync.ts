import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useEffect, useRef } from 'react';
import { WebApp } from '../lib/telegram';
import { readingPositionsApi } from '../api/readingPositions';

const resolveUserId = (): string => {
  const tgId = WebApp.initDataUnsafe?.user?.id;
  return tgId ? String(tgId) : '12345';
};

// Ждёт пока страница станет достаточно высокой, потом скроллит
function restoreScroll(ratio: number, attempts = 0) {
  if (ratio < 0.01) return;
  const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
  if (maxScroll > 50) {
    window.scrollTo({ top: ratio * maxScroll, behavior: 'instant' });
  } else if (attempts < 20) {
    setTimeout(() => restoreScroll(ratio, attempts + 1), 100);
  }
}

export const useScrollSync = (topicId: string | undefined, isContentReady: boolean) => {
  const connectionRef = useRef<ReturnType<typeof buildConnection> | null>(null);
  const savedRatioRef = useRef<number | null>(null);
  const restoredRef = useRef(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Подключение и получение сохранённой позиции
  useEffect(() => {
    if (!topicId) return;

    restoredRef.current = false;
    savedRatioRef.current = null;

    const userId = resolveUserId();
    const connection = buildConnection(userId);
    connectionRef.current = connection;

    let stopped = false;

    connection.start().catch(() => { });

    // Получаем позицию независимо от состояния соединения
    readingPositionsApi.get(topicId)
      .then(({ scrollRatio }) => {
        if (!stopped) savedRatioRef.current = scrollRatio;
      })
      .catch(() => { });

    const onScroll = () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
      debounceRef.current = setTimeout(() => {
        const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
        if (maxScroll <= 0) return;
        const ratio = Math.min(1, window.scrollY / maxScroll);
        if (connection.state === HubConnectionState.Connected) {
          connection.invoke('SaveScrollPosition', topicId, ratio).catch(() => { });
        }
      }, 800);
    };

    window.addEventListener('scroll', onScroll, { passive: true });

    return () => {
      stopped = true;
      window.removeEventListener('scroll', onScroll);
      if (debounceRef.current) clearTimeout(debounceRef.current);
      connection.stop();
    };
  }, [topicId]);

  // Восстанавливаем скролл только когда контент реально отрисован
  useEffect(() => {
    if (!isContentReady || restoredRef.current) return;
    restoredRef.current = true;

    const ratio = savedRatioRef.current;
    if (ratio !== null) {
      restoreScroll(ratio);
    } else {
      // Позиция ещё не пришла — ждём немного
      const timer = setTimeout(() => {
        if (savedRatioRef.current !== null) restoreScroll(savedRatioRef.current);
      }, 300);
      return () => clearTimeout(timer);
    }
  }, [isContentReady]);
};

function buildConnection(userId: string) {
  return new HubConnectionBuilder()
    .withUrl(`/hubs/reading?userId=${userId}`)
    .withAutomaticReconnect()
    .build();
}
