import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Download } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { sectionsApi } from '../api/sections';
import { exportApi } from '../api/export';
import { SectionCard } from '../components/SectionCard';
import { Button } from '../components/ui/Button';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
import { Spinner } from '../components/ui/Spinner';

const EMOJI_OPTIONS = ['📚', '💻', '🧠', '🔬', '🎯', '🌐', '📐', '⚡', '🗂️', '🔧'];

export const HomePage = () => {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ title: '', description: '', icon: '📚' });

  const { data: sections, isLoading } = useQuery({
    queryKey: ['sections'],
    queryFn: sectionsApi.getAll,
  });

  const create = useMutation({
    mutationFn: () => sectionsApi.create(form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sections'] }); setOpen(false); setForm({ title: '', description: '', icon: '📚' }); },
  });

  if (isLoading) return <Spinner />;

  return (
    <div className="min-h-screen pb-24">
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold">Справочник</h1>
            <p className="text-sm opacity-50 mt-0.5">{sections?.length ?? 0} разделов</p>
          </div>
          <div className="flex gap-2">
            <button onClick={() => exportApi.exportAll()} className="w-10 h-10 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
              <Download size={18} className="opacity-60" />
            </button>
            <button onClick={() => setOpen(true)} className="w-10 h-10 flex items-center justify-center rounded-xl bg-[var(--tg-theme-button-color,#2481cc)] text-white">
              <Plus size={20} />
            </button>
          </div>
        </div>

        {sections?.length === 0
          ? <EmptyState icon="📚" title="Пока нет разделов" subtitle="Создай первый раздел и начни добавлять темы" action={{ label: 'Создать раздел', onClick: () => setOpen(true) }} />
          : <div className="flex flex-col gap-3">
              {sections?.map(s => <SectionCard key={s.id} section={s} onClick={() => navigate(`/sections/${s.id}`)} />)}
            </div>
        }
      </div>

      <Modal open={open} title="Новый раздел" onClose={() => setOpen(false)}
        actions={<>
          <Button variant="secondary" fullWidth onClick={() => setOpen(false)}>Отмена</Button>
          <Button fullWidth loading={create.isPending} onClick={() => create.mutate()}>Создать</Button>
        </>}
      >
        <div className="flex gap-2 mb-3 flex-wrap">
          {EMOJI_OPTIONS.map(e => (
            <button key={e} onClick={() => setForm(f => ({ ...f, icon: e }))}
              className={`text-2xl p-1.5 rounded-xl transition-colors ${form.icon === e ? 'bg-[var(--tg-theme-button-color,#2481cc)]' : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]'}`}>
              {e}
            </button>
          ))}
        </div>
        <div className="flex flex-col gap-3">
          <Input label="Название" placeholder="Например: Golang" value={form.title} onChange={e => setForm(f => ({ ...f, title: e.target.value }))} />
          <Input label="Описание (необязательно)" placeholder="Краткое описание раздела" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} />
        </div>
      </Modal>
    </div>
  );
};
