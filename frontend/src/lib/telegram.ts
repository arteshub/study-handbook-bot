const tg = typeof window !== 'undefined' ? (window as any)?.Telegram?.WebApp : null;

export const WebApp = {
  ready: () => { try { tg?.ready?.(); } catch { /* not in Telegram */ } },
  expand: () => { try { tg?.expand?.(); } catch { /* not in Telegram */ } },
  initData: (tg?.initData ?? '') as string,
  initDataUnsafe: (tg?.initDataUnsafe ?? {}) as { user?: { id?: number } },
};
