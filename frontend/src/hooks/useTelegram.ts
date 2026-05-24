import { useEffect } from 'react';
import WebApp from '@twa-dev/sdk';

export const useTelegram = () => {
  useEffect(() => {
    WebApp.ready();
    WebApp.expand();
  }, []);

  return {
    webApp: WebApp,
    user: WebApp.initDataUnsafe?.user,
    colorScheme: WebApp.colorScheme,
  };
};
