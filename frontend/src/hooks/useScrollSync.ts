import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { useEffect, useRef } from 'react';
import { WebApp } from '../lib/telegram';
import { readingPositionsApi } from '../api/readingPositions';

const resolveUserId = (): string => {
  const tgId = WebApp.initDataUnsafe?.user?.id;
  return tgId ? String(tgId) : '12345';
};

export const useScrollSync = (topicId: string | undefined) => {
  const connectionRef = useRef<ReturnType<typeof buildConnection> | null>(null);
  const restoredRef = useRef(false);
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!topicId) return;

    const userId = resolveUserId();
    const connection = buildConnection(userId);
    connectionRef.current = connection;

    let stopped = false;

    connection.start().then(async () => {
      if (stopped) return;
      // Restore scroll position after data loads
      if (!restoredRef.current) {
        restoredRef.current = true;
        try {
          const { scrollRatio } = await readingPositionsApi.get(topicId);
          if (scrollRatio > 0.01) {
            // Wait for content to render
            requestAnimationFrame(() => {
              const maxScroll = document.documentElement.scrollHeight - window.innerHeight;
              window.scrollTo({ top: scrollRatio * maxScroll, behavior: 'instant' });
            });
          }
        } catch { /* ignore */ }
      }
    }).catch(() => { /* connection failed, ignore */ });

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
};

function buildConnection(userId: string) {
  return new HubConnectionBuilder()
    .withUrl(`/hubs/reading?userId=${userId}`)
    .withAutomaticReconnect()
    .build();
}
