import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import MDEditor from '@uiw/react-md-editor';
import { ArrowLeft, Save } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { topicsApi } from '../api/topics';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Spinner } from '../components/ui/Spinner';

export const TopicEditorPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data: topic, isLoading } = useQuery({
    queryKey: ['topics', id, 'detail'],
    queryFn: () => topicsApi.getById(id!),
  });

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [summary, setSummary] = useState('');

  useEffect(() => {
    if (topic) {
      setTitle(topic.title);
      setContent(topic.content);
      setSummary(topic.summary ?? '');
    }
  }, [topic]);

  const save = useMutation({
    mutationFn: () => topicsApi.update(id!, { title, content, summary }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['topics', id, 'detail'] });
      navigate(`/topics/${id}`);
    },
  });

  if (isLoading) return <Spinner />;

  return (
    <div className="min-h-screen pb-24">
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center gap-3 mb-5">
          <button onClick={() => navigate(-1)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <ArrowLeft size={18} />
          </button>
          <h1 className="text-xl font-bold flex-1">Редактор</h1>
          <Button size="sm" loading={save.isPending} onClick={() => save.mutate()}>
            <Save size={14} className="mr-1 inline" /> Сохранить
          </Button>
        </div>

        <div className="flex flex-col gap-4">
          <Input label="Заголовок темы" value={title} onChange={e => setTitle(e.target.value)} />
          <Input label="Краткое резюме (для теста)" placeholder="В чём суть темы за 1 предложение?" value={summary} onChange={e => setSummary(e.target.value)} />

          <div>
            <label className="text-sm font-medium opacity-70 block mb-1">Содержание (Markdown)</label>
            <div data-color-mode="light">
              <MDEditor value={content} onChange={v => setContent(v ?? '')} height={400} preview="edit" />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
