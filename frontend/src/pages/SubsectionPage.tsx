import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
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

export const SubsectionPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [open, setOpen] = useState(false);
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
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ['topics', id] });
      setOpen(false);
      navigate(`/topics/${data.id}/edit`);
    },
  });

  if (isLoading) return <Spinner />;

  return (
    <div className="min-h-screen pb-24">
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center gap-3 mb-6">
          <button onClick={() => navigate(-1)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <ArrowLeft size={18} />
          </button>
          <div className="flex-1">
            <h1 className="text-xl font-bold">Подраздел</h1>
            <p className="text-sm opacity-50">{topics?.length ?? 0} тем</p>
          </div>
          <button onClick={() => deleteSub.mutate()} className="w-9 h-9 flex items-center justify-center rounded-xl bg-red-50">
            <Trash2 size={16} className="text-red-400" />
          </button>
        </div>

        <div className="flex justify-between items-center mb-3">
          <span className="text-sm font-medium opacity-60">Темы</span>
          <button onClick={() => setOpen(true)} className="flex items-center gap-1 text-sm text-[var(--tg-theme-link-color,#2481cc)] font-medium">
            <Plus size={14} /> Добавить
          </button>
        </div>

        {topics?.length === 0
          ? <EmptyState icon="📝" title="Нет тем" subtitle="Добавь первую тему с материалом" action={{ label: 'Добавить тему', onClick: () => setOpen(true) }} />
          : <div className="flex flex-col gap-3">
              {topics?.map(t => <TopicCard key={t.id} topic={t} onClick={() => navigate(`/topics/${t.id}`)} />)}
            </div>
        }
      </div>

      <Modal open={open} title="Новая тема" onClose={() => setOpen(false)}
        actions={<>
          <Button variant="secondary" fullWidth onClick={() => setOpen(false)}>Отмена</Button>
          <Button fullWidth loading={create.isPending} onClick={() => create.mutate()}>Создать и редактировать</Button>
        </>}
      >
        <Input label="Название темы" placeholder="Например: Срезы (slices)" value={title} onChange={e => setTitle(e.target.value)} />
      </Modal>
    </div>
  );
};
