import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Download, Plus, Trash2 } from 'lucide-react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { sectionsApi } from '../api/sections';
import { subsectionsApi } from '../api/subsections';
import { exportApi } from '../api/export';
import { SubsectionCard } from '../components/SubsectionCard';
import { Button } from '../components/ui/Button';
import { EmptyState } from '../components/ui/EmptyState';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
import { Spinner } from '../components/ui/Spinner';

export const SectionPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState({ title: '', description: '' });

  const { data: section } = useQuery({ queryKey: ['sections', id], queryFn: () => sectionsApi.getById(id!) });
  const { data: subsections, isLoading } = useQuery({ queryKey: ['subsections', id], queryFn: () => subsectionsApi.getBySectionId(id!) });

  const create = useMutation({
    mutationFn: () => subsectionsApi.create(id!, form),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['subsections', id] }); setOpen(false); setForm({ title: '', description: '' }); },
  });

  const deleteSection = useMutation({
    mutationFn: () => sectionsApi.delete(id!),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['sections'] }); navigate('/'); },
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
            <h1 className="text-xl font-bold flex items-center gap-2">{section?.icon} {section?.title}</h1>
            {section?.description && <p className="text-sm opacity-50">{section.description}</p>}
          </div>
          <button onClick={() => exportApi.exportSection(id!, section?.title ?? 'section')} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <Download size={16} className="opacity-60" />
          </button>
          <button onClick={() => deleteSection.mutate()} className="w-9 h-9 flex items-center justify-center rounded-xl bg-red-50">
            <Trash2 size={16} className="text-red-400" />
          </button>
        </div>

        <div className="flex justify-between items-center mb-3">
          <span className="text-sm font-medium opacity-60">{subsections?.length ?? 0} подразделов</span>
          <button onClick={() => setOpen(true)} className="flex items-center gap-1 text-sm text-[var(--tg-theme-link-color,#2481cc)] font-medium">
            <Plus size={14} /> Добавить
          </button>
        </div>

        {subsections?.length === 0
          ? <EmptyState icon="🗂️" title="Нет подразделов" action={{ label: 'Добавить подраздел', onClick: () => setOpen(true) }} />
          : <div className="flex flex-col gap-3">
              {subsections?.map(s => <SubsectionCard key={s.id} subsection={s} onClick={() => navigate(`/subsections/${s.id}`)} />)}
            </div>
        }
      </div>

      <Modal open={open} title="Новый подраздел" onClose={() => setOpen(false)}
        actions={<>
          <Button variant="secondary" fullWidth onClick={() => setOpen(false)}>Отмена</Button>
          <Button fullWidth loading={create.isPending} onClick={() => create.mutate()}>Создать</Button>
        </>}
      >
        <div className="flex flex-col gap-3">
          <Input label="Название" placeholder="Например: Массивы и слайсы" value={form.title} onChange={e => setForm(f => ({ ...f, title: e.target.value }))} />
          <Input label="Описание (необязательно)" placeholder="Краткое описание" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} />
        </div>
      </Modal>
    </div>
  );
};
