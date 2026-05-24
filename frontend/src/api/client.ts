import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_URL ?? '/api';

export const apiClient = axios.create({ baseURL: BASE_URL });

export const setUserId = (id: number) => {
  apiClient.defaults.headers.common['X-Telegram-User-Id'] = String(id);
};
