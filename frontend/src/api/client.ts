import axios from 'axios';
import { WebApp } from '../lib/telegram';

const BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

const resolveUserId = (): string => {
  const tgId = WebApp.initDataUnsafe?.user?.id;
  return tgId ? String(tgId) : '12345';
};

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: { 'X-Telegram-User-Id': resolveUserId() },
});

export const setUserId = (id: number) => {
  apiClient.defaults.headers.common['X-Telegram-User-Id'] = String(id);
};
