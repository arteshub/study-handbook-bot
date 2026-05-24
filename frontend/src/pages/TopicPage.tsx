import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Download, Edit, Trash2 } from 'lucide-react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { topicsApi } from '../api/topics';
import { exportApi } from '../api/export';

import { Spinner } from '../components/ui/Spinner';

export const TopicPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data: topic, isLoading } = useQuery({
    queryKey: ['topics', id, 'detail'],
    queryFn: () => topicsApi.getById(id!),
  });

  const deleteTopic = useMutation({
    mutationFn: () => topicsApi.delete(id!),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['topics'] }); navigate(-1); },
  });

  if (isLoading) return <Spinner />;
  if (!topic) return null;

  return (
    <div className="min-h-screen pb-24">
      <div className="px-4 pt-6 pb-4">
        <div className="flex items-center gap-2 mb-5">
          <button onClick={() => navigate(-1)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] shrink-0">
            <ArrowLeft size={18} />
          </button>
          <div className="flex-1" />
          <button onClick={() => exportApi.exportTopic(id!, topic.title)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <Download size={16} className="opacity-60" />
          </button>
          <button onClick={() => navigate(`/topics/${id}/edit`)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <Edit size={16} className="opacity-60" />
          </button>
          <button onClick={() => deleteTopic.mutate()} className="w-9 h-9 flex items-center justify-center rounded-xl bg-red-50">
            <Trash2 size={16} className="text-red-400" />
          </button>
        </div>

        <h1 className="text-2xl font-bold mb-2">{topic.title}</h1>
        {topic.summary && (
          <p className="text-sm p-3 rounded-xl bg-[var(--tg-theme-button-color,#2481cc)]/10 text-[var(--tg-theme-link-color,#2481cc)] mb-4">
            💡 {topic.summary}
          </p>
        )}

        <div className="prose prose-sm max-w-none text-[var(--tg-theme-text-color,#000)]">
          <ReactMarkdown>{topic.content}</ReactMarkdown>
        </div>
      </div>
    </div>
  );
};
