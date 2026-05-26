import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';
import { WebApp } from '../lib/telegram';
import { readingPositionsApi } from '../api/readingPositions';

const resolveUserId = (): string => {
  const tgId = WebApp.initDataUnsafe?.user?.id;
  return tgId ? String(tgId) : '12345';
};

// Ждёт пока высота страницы стабилизируется (контент отрисован), потом скроллит
function restoreScroll(ratio: number) {
  if (ratio < 0.01) return;

  let prevHeight = 0;
  let sameCount = 0;
  let elapsed = 0;

  const tick = () => {
    const h = document.documentElement.scrollHeight;
    const maxScroll = h - window.innerHeight;

    if (h > window.innerHeight && h === prevHeight) {
      sameCount++;
    } else {
      sameCount = 0;
      prevHeight = h;
    }

    if (sameCount >= 3) {
      // Высота стабильна ≥150ms — теперь можно скроллить
      window.scrollTo({ top: Math.round(ratio * maxScroll), behavior: 'instant' });
      return;
    }

    if (elapsed < 3000) {
      elapsed += 50;
      setTimeout(tick, 50);
    }
  };

  setTimeout(tick, 50);
}

export const useScrollSync = (topicId: string | undefined, isContentReady: boolean) => {
  // null = ещё не загружена, число = загружена (0 = начало)
  const [savedRatio, setSavedRatio] = useState<number | null>(null);
  const restoredRef = useRef(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // При смене темы — сбрасываем состояние и грузим позицию
  useEffect(() => {
    if (!topicId) return;
    restoredRef.current = false;
    setSavedRatio(null);

    readingPositionsApi.get(topicId)
      .then(({ scrollRatio }) => setSavedRatio(scrollRatio))
      .catch(() => setSavedRatio(0));
  }, [topicId]);

  // SignalR соединение и сохранение скролла
  useEffect(() => {
    if (!topicId) return;

    const userId = resolveUserId();
    const connection = buildConnection(userId);
    connection.start().catch(() => { });

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
      window.removeEventListener('scroll', onScroll);
      if (debounceRef.current) clearTimeout(debounceRef.current);
      connection.stop();
    };
  }, [topicId]);

  // Восстанавливаем когда оба готовы: контент отрисован И позиция загружена
  useEffect(() => {
    if (!isContentReady || savedRatio === null || restoredRef.current) return;
    restoredRef.current = true;
    restoreScroll(savedRatio);
  }, [isContentReady, savedRatio]);
};

function buildConnection(userId: string) {
  return new HubConnectionBuilder()
    .withUrl(`/hubs/reading?userId=${userId}`)
    .withAutomaticReconnect()
    .build();
}
