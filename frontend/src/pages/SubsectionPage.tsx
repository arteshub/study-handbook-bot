import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, CirclePlay, FileText, Folder, PenLine, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { subsectionsApi } from '../api/subsections';
import { topicsApi } from '../api/topics';
import { youtubeApi } from '../api/youtube';
import { TopicCard } from '../components/TopicCard';
import { Button } from '../components/ui/Button';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
import { Spinner } from '../components/ui/Spinner';

type AddMode = 'topic' | 'folder' | null;
type CreateMode = null | 'manual' | 'youtube';

export const SubsectionPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const [addMode, setAddMode] = useState<AddMode>(null);
  const [createMode, setCreateMode] = useState<CreateMode>(null);
  const [title, setTitle] = useState('');
  const [ytUrl, setYtUrl] = useState('');

  const { data: topics, isLoading } = useQuery({
    queryKey: ['topics', id],
    queryFn: () => topicsApi.getBySubsectionId(id!),
  });

  const deleteSub = useMutation({
    mutationFn: () => subsectionsApi.delete(id!),
    onSuccess: () => navigate(-1),
  });

  const createManual = useMutation({
    mutationFn: () => topicsApi.create(id!, { title, content: '', summary: '' }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['topics', id] });
      closeTopicModal();
    },
  });

  const createFromYoutube = useMutation({
    mutationFn: async () => {
      const result = await youtubeApi.extract(ytUrl);
      return topicsApi.create(id!, { title: result.title, content: result.content, summary: '' });
    },
    onSuccess: data => {
      qc.invalidateQueries({ queryKey: ['topics', id] });
      closeTopicModal();
      navigate(`/topics/${data.id}/edit`);
    },
  });

  const closeTopicModal = () => {
    if (createFromYoutube.isPending) return;
    setAddMode(null);
    setCreateMode(null);
    setTitle('');
    setYtUrl('');
    createManual.reset();
    createFromYoutube.reset();
  };

  if (isLoading) return <Spinner />;

  return (
    <div className="min-h-screen pb-24">
      <div className="sticky top-0 z-10 bg-[var(--tg-theme-bg-color,#fff)] border-b border-black/5 px-4 py-3 flex items-center gap-2">
        <button onClick={() => navigate(-1)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] shrink-0">
          <ArrowLeft size={18} />
        </button>
        <span className="flex-1 text-base font-bold">Подраздел</span>
        <button
          onClick={() => setAddMode('folder')}
          className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
          title="Добавить папку"
        >
          <Folder size={16} className="text-[var(--tg-theme-link-color,#2481cc)]" />
        </button>
        <button
          onClick={() => setAddMode('topic')}
          className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-button-color,#2481cc)] text-white"
          title="Добавить тему"
        >
          <Plus size={18} />
        </button>
        <button onClick={() => deleteSub.mutate()} className="w-9 h-9 flex items-center justify-center rounded-xl bg-red-50">
          <Trash2 size={16} className="text-red-400" />
        </button>
      </div>

      <div className="px-4 pt-4">
        {topics?.length === 0
          ? (
            <EmptyState
              icon="📝"
              title="Пусто"
              subtitle="Добавь папку для группировки или тему с материалом"
              action={{ label: 'Добавить тему', onClick: () => setAddMode('topic') }}
            />
          )
          : (
            <div className="flex flex-col gap-2">
              {topics?.map(t => (
                <TopicCard key={t.id} topic={t} onClick={() => navigate(`/topics/${t.id}`)} />
              ))}
            </div>
          )
        }
      </div>

      {/* Add folder modal */}
      <Modal
        open={addMode === 'folder'}
        title="Новая папка"
        onClose={() => { setAddMode(null); setTitle(''); }}
        actions={<>
          <Button variant="secondary" fullWidth onClick={() => { setAddMode(null); setTitle(''); }}>Отмена</Button>
          <Button fullWidth loading={createManual.isPending} onClick={() => createManual.mutate()}>Создать</Button>
        </>}
      >
        <div className="flex items-center gap-2 mb-3 p-3 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
          <Folder size={16} className="text-[var(--tg-theme-link-color,#2481cc)]" />
          <p className="text-sm opacity-70">Папка для группировки подтем</p>
        </div>
        <Input label="Название" placeholder="Например: Продвинутые концепции" value={title} onChange={e => setTitle(e.target.value)} />
      </Modal>

      {/* Add topic modal */}
      <Modal
        open={addMode === 'topic'}
        title="Новая тема"
        onClose={closeTopicModal}
      >
        {createMode === null && (
          <div className="flex flex-col gap-3">
            <button
              onClick={() => setCreateMode('manual')}
              className="flex items-center gap-3 p-4 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] active:opacity-70 transition-opacity text-left"
            >
              <div className="w-10 h-10 rounded-xl bg-[var(--tg-theme-button-color,#2481cc)] flex items-center justify-center shrink-0">
                <PenLine size={18} className="text-white" />
              </div>
              <div>
                <p className="font-semibold text-sm">Самостоятельно</p>
                <p className="text-xs opacity-60 mt-0.5">Создать тему и вставить текст вручную</p>
              </div>
            </button>

            <button
              onClick={() => setCreateMode('youtube')}
              className="flex items-center gap-3 p-4 rounded-xl bg-red-50 active:opacity-70 transition-opacity text-left"
            >
              <div className="w-10 h-10 rounded-xl bg-red-500 flex items-center justify-center shrink-0">
                <CirclePlay size={18} className="text-white" />
              </div>
              <div>
                <p className="font-semibold text-sm">Из YouTube</p>
                <p className="text-xs opacity-60 mt-0.5">Сгенерировать статью по субтитрам видео</p>
              </div>
            </button>
          </div>
        )}

        {createMode === 'manual' && (
          <>
            <Input
              label="Название темы"
              placeholder="Например: Срезы (slices)"
              value={title}
              onChange={e => setTitle(e.target.value)}
            />
            <div className="flex gap-2 mt-4">
              <Button variant="secondary" className="flex-1" onClick={() => setCreateMode(null)}>Назад</Button>
              <Button
                className="flex-1"
                loading={createManual.isPending}
                disabled={!title.trim()}
                onClick={() => createManual.mutate()}
              >
                Создать
              </Button>
            </div>
          </>
        )}

        {createMode === 'youtube' && (
          <>
            <Input
              label="Ссылка на видео"
              placeholder="https://youtube.com/watch?v=..."
              value={ytUrl}
              onChange={e => setYtUrl(e.target.value)}
              disabled={createFromYoutube.isPending}
            />
            {createFromYoutube.isPending && (
              <p className="text-sm opacity-60 mt-3 text-center">
                Извлекаю субтитры и генерирую статью — может занять несколько минут
              </p>
            )}
            {createFromYoutube.isError && (
              <p className="text-sm text-red-500 mt-3">
                Не удалось сгенерировать. Проверь ссылку и наличие субтитров у видео.
              </p>
            )}
            <div className="flex gap-2 mt-4">
              <Button variant="secondary" className="flex-1" onClick={() => setCreateMode(null)} disabled={createFromYoutube.isPending}>Назад</Button>
              <Button
                className="flex-1"
                loading={createFromYoutube.isPending}
                disabled={!ytUrl.trim() || createFromYoutube.isPending}
                onClick={() => createFromYoutube.mutate()}
              >
                Создать
              </Button>
            </div>
          </>
        )}
      </Modal>
    </div>
  );
};
