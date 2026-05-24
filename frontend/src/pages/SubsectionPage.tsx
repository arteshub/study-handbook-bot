import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, FileText, Folder, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { subsectionsApi } from '../api/subsections';
import { topicsApi } from '../api/topics';
import { TopicCard } from '../components/TopicCard';
import { Button } from '../components/ui/Button';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
import { Spinner } from '../components/ui/Spinner';

type AddMode = 'topic' | 'folder' | null;

export const SubsectionPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [addMode, setAddMode] = useState<AddMode>(null);
  const [title, setTitle] = useState('');

  const { data: topics, isLoading } = useQuery({
    queryKey: ['topics', id],
    queryFn: () => topicsApi.getBySubsectionId(id!),
  });

  const deleteSub = useMutation({
    mutationFn: () => subsectionsApi.delete(id!),
    onSuccess: () => navigate(-1),
  });

  const create = useMutation({
    mutationFn: () => topicsApi.create(id!, { title, content: '', summary: '' }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['topics', id] });
      setAddMode(null);
      setTitle('');
    },
  });

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
          <Button fullWidth loading={create.isPending} onClick={() => create.mutate()}>Создать</Button>
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
        onClose={() => { setAddMode(null); setTitle(''); }}
        actions={<>
          <Button variant="secondary" fullWidth onClick={() => { setAddMode(null); setTitle(''); }}>Отмена</Button>
          <Button fullWidth loading={create.isPending} onClick={() => create.mutate()}>Создать</Button>
        </>}
      >
        <div className="flex items-center gap-2 mb-3 p-3 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
          <FileText size={16} className="opacity-50" />
          <p className="text-sm opacity-70">Тема с текстом и справочным материалом</p>
        </div>
        <Input label="Название темы" placeholder="Например: Срезы (slices)" value={title} onChange={e => setTitle(e.target.value)} />
      </Modal>
    </div>
  );
};
