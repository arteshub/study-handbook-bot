import { useMutation } from '@tanstack/react-query';
import { useState } from 'react';
import { apiClient } from '../api/client';
import { Button } from '../components/ui/Button';
import { Card } from '../components/ui/Card';
import { Input } from '../components/ui/Input';

export const SettingsPage = () => {
  const [apiKey, setApiKey] = useState('');
  const [saved, setSaved] = useState(false);

  const save = useMutation({
    mutationFn: () => apiClient.put('/users/openai-key', { apiKey }),
    onSuccess: () => { setSaved(true); setTimeout(() => setSaved(false), 2000); },
  });

  return (
    <div className="px-4 pt-6 pb-24 min-h-screen">
      <h1 className="text-2xl font-bold mb-2">Настройки</h1>
      <p className="text-sm opacity-50 mb-6">Конфигурация приложения</p>

      <Card className="mb-4">
        <p className="font-semibold mb-1">🤖 ChatGPT API ключ</p>
        <p className="text-sm opacity-60 mb-3">Нужен для AI-режима тестов. Ключ хранится в зашифрованном виде.</p>
        <Input
          type="password"
          placeholder="sk-..."
          value={apiKey}
          onChange={e => setApiKey(e.target.value)}
        />
        <Button fullWidth className="mt-3" loading={save.isPending} onClick={() => save.mutate()}>
          {saved ? '✅ Сохранено' : 'Сохранить ключ'}
        </Button>
      </Card>

      <Card>
        <p className="font-semibold mb-1">ℹ️ О приложении</p>
        <p className="text-sm opacity-60">Справочник знаний v1.0</p>
        <p className="text-sm opacity-40 mt-1">Создавай разделы, изучай темы, проверяй себя</p>
      </Card>
    </div>
  );
};
