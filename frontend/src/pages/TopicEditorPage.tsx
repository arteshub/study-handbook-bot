import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import MDEditor from '@uiw/react-md-editor';
import { ArrowLeft, Save, CirclePlay } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { topicsApi } from '../api/topics';
import { youtubeApi } from '../api/youtube';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
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
  const [ytOpen, setYtOpen] = useState(false);
  const [ytUrl, setYtUrl] = useState('');

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
      navigate(`/topics/${id}`, { replace: true });
    },
  });

  const extractYoutube = useMutation({
    mutationFn: () => youtubeApi.extract(ytUrl),
    onSuccess: result => {
      setContent(result.content);
      if (!title.trim()) setTitle(result.title);
      setYtOpen(false);
      setYtUrl('');
    },
  });

  const closeYtModal = () => {
    if (extractYoutube.isPending) return;
    setYtOpen(false);
    setYtUrl('');
    extractYoutube.reset();
  };

  if (isLoading) return <Spinner />;

  return (
    <div className="min-h-screen pb-24">
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center gap-3 mb-5">
          <button
            onClick={() => navigate(-1)}
            className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
          >
            <ArrowLeft size={18} />
          </button>
          <h1 className="text-xl font-bold flex-1">Редактор</h1>
          <Button size="sm" loading={save.isPending} onClick={() => save.mutate()}>
            <Save size={14} className="mr-1 inline" /> Сохранить
          </Button>
        </div>

        <div className="flex flex-col gap-4">
          <Input label="Заголовок темы" value={title} onChange={e => setTitle(e.target.value)} />
          <Input
            label="Краткое резюме (для теста)"
            placeholder="В чём суть темы за 1 предложение?"
            value={summary}
            onChange={e => setSummary(e.target.value)}
          />

          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="text-sm font-medium opacity-70">Содержание (Markdown)</label>
              <button
                onClick={() => setYtOpen(true)}
                className="flex items-center gap-1.5 text-xs font-medium text-red-500 active:opacity-70 transition-opacity"
              >
                <CirclePlay size={14} />
                Из YouTube
              </button>
            </div>
            <div data-color-mode="light">
              <MDEditor value={content} onChange={v => setContent(v ?? '')} height={400} preview="edit" />
            </div>
          </div>
        </div>
      </div>

      <Modal
        open={ytOpen}
        title="Сгенерировать из YouTube"
        onClose={closeYtModal}
        actions={
          <>
            <Button variant="secondary" className="flex-1" onClick={closeYtModal} disabled={extractYoutube.isPending}>
              Отмена
            </Button>
            <Button
              className="flex-1"
              loading={extractYoutube.isPending}
              disabled={!ytUrl.trim() || extractYoutube.isPending}
              onClick={() => extractYoutube.mutate()}
            >
              Сгенерировать
            </Button>
          </>
        }
      >
        <Input
          label="Ссылка на видео"
          placeholder="https://youtube.com/watch?v=..."
          value={ytUrl}
          onChange={e => setYtUrl(e.target.value)}
          disabled={extractYoutube.isPending}
        />
        {extractYoutube.isPending && (
          <p className="text-sm opacity-60 mt-3 text-center">
            Извлекаю субтитры и генерирую статью — может занять несколько минут
          </p>
        )}
        {extractYoutube.isError && (
          <p className="text-sm text-red-500 mt-3">
            Не удалось сгенерировать. Проверь ссылку и наличие субтитров у видео.
          </p>
        )}
      </Modal>
    </div>
  );
};
