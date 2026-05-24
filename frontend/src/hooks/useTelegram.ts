import { useEffect } from 'react';
import { WebApp } from '../lib/telegram';

export const useTelegram = () => {
  useEffect(() => {
    WebApp.ready();
    WebApp.expand();
  }, []);

  return {
    user: WebApp.initDataUnsafe?.user,
  };
};
