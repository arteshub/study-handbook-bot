import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';
import { WebApp } from '../lib/telegram';
import { readingPositionsApi } from '../api/readingPositions';

const resolveUserId = (): string => {
  const tgId = WebApp.initDataUnsafe?.user?.id;
  return tgId ? String(tgId) : '12345';
};

function restoreScroll(ratio: number) {
  if (ratio < 0.01) return;

  let attempts = 0;

  const tick = () => {
    const scrollH = document.documentElement.scrollHeight;
    const viewH = window.innerHeight;
    const maxScroll = scrollH - viewH;
    const target = Math.round(ratio * maxScroll);

    console.log(`[scrollSync] attempt=${attempts} ratio=${ratio} scrollH=${scrollH} viewH=${viewH} maxScroll=${maxScroll} target=${target} currentY=${window.scrollY}`);

    if (maxScroll > 50) {
      // Пробуем все известные методы скролла
      try { window.scrollTo(0, target); } catch { /* */ }
      try { document.documentElement.scrollTop = target; } catch { /* */ }
      try { document.body.scrollTop = target; } catch { /* */ }

      console.log(`[scrollSync] after scroll: scrollY=${window.scrollY}`);

      // Если позиция всё равно неправильная — повторяем ещё раз через 200ms
      // (на случай если что-то сбрасывает скролл после нас)
      if (attempts < 8 && Math.abs(window.scrollY - target) > 30) {
        attempts++;
        setTimeout(tick, 200);
      }
      return;
    }

    // Контент ещё не отрисован — ждём
    if (attempts < 30) {
      attempts++;
      setTimeout(tick, 100);
    }
  };

  setTimeout(tick, 150);
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
