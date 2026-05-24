import { Card } from '../components/ui/Card';

export const SettingsPage = () => (
  <div className="px-4 pt-6 pb-24 min-h-screen">
    <h1 className="text-2xl font-bold mb-2">Настройки</h1>
    <p className="text-sm opacity-50 mb-6">Конфигурация приложения</p>

    <Card className="mb-4">
      <p className="font-semibold mb-1">🤖 AI-тесты</p>
      <p className="text-sm opacity-60">ChatGPT уже подключён и готов к работе. Запускай тест из любой темы или подраздела.</p>
    </Card>

    <Card>
      <p className="font-semibold mb-1">ℹ️ О приложении</p>
      <p className="text-sm opacity-60">Справочник знаний v1.0</p>
      <p className="text-sm opacity-40 mt-1">Создавай разделы, изучай темы, проверяй себя</p>
    </Card>
  </div>
);
