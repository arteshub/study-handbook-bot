import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

export const apiClient = axios.create({ baseURL: BASE_URL });

// Read auth header lazily so Telegram.WebApp is guaranteed to be initialized
apiClient.interceptors.request.use(config => {
  const tg = (window as any)?.Telegram?.WebApp;
  const initData: string = tg?.initData ?? '';

  if (initData) {
    config.headers['X-Telegram-Init-Data'] = initData;
  } else {
    // Dev fallback: plain user ID accepted by backend only in Development
    const devId = tg?.initDataUnsafe?.user?.id;
    config.headers['X-Telegram-User-Id'] = String(devId ?? '12345');
  }

  return config;
});

export const setUserId = (id: number) => {
  apiClient.defaults.headers.common['X-Telegram-User-Id'] = String(id);
};
