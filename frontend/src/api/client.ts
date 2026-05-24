import axios from 'axios';
import { WebApp } from '../lib/telegram';

const BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

export const apiClient = axios.create({ baseURL: BASE_URL });

// Set auth headers once on init
if (WebApp.initData) {
  apiClient.defaults.headers.common['X-Telegram-Init-Data'] = WebApp.initData;
} else {
  // Dev fallback: use unsafe user ID (Development only)
  const devId = WebApp.initDataUnsafe?.user?.id;
  apiClient.defaults.headers.common['X-Telegram-User-Id'] = String(devId ?? '12345');
}

export const setUserId = (id: number) => {
  apiClient.defaults.headers.common['X-Telegram-User-Id'] = String(id);
};
