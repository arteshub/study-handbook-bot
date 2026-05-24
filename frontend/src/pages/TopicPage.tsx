import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Download, Edit, ExternalLink, FolderPlus, Link, Plus, Trash2, X } from 'lucide-react';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import MDEditor from '@uiw/react-md-editor';
import { topicsApi } from '../api/topics';
import { exportApi } from '../api/export';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Modal } from '../components/ui/Modal';
import { Spinner } from '../components/ui/Spinner';

type Panel = 'links' | 'children' | null;

export const TopicPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const [panel, setPanel] = useState<Panel>(null);
  const [childTitle, setChildTitle] = useState('');
  const [linkForm, setLinkForm] = useState({ title: '', url: '' });

  const { data: topic, isLoading } = useQuery({
    queryKey: ['topics', id, 'detail'],
    queryFn: () => topicsApi.getById(id!),
  });

  const { data: children } = useQuery({
    queryKey: ['topics', id, 'children'],
    queryFn: () => topicsApi.getBySubsectionId(topic!.subsectionId, id!),
    enabled: !!topic,
  });

  const deleteTopic = useMutation({
    mutationFn: () => topicsApi.delete(id!),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['topics'] }); navigate(-1); },
  });

  const createChild = useMutation({
    mutationFn: () => topicsApi.create(topic!.subsectionId, { title: childTitle, content: '', summary: '', parentTopicId: id }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['topics', id, 'children'] });
      qc.invalidateQueries({ queryKey: ['topics', id, 'detail'] });
      setPanel(null);
      setChildTitle('');
    },
  });

  const addLink = useMutation({
    mutationFn: () => topicsApi.addLink(id!, linkForm),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['topics', id, 'detail'] });
      setPanel(null);
      setLinkForm({ title: '', url: '' });
    },
  });

  const deleteLink = useMutation({
    mutationFn: (linkId: string) => topicsApi.deleteLink(id!, linkId),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['topics', id, 'detail'] }),
  });

  if (isLoading) return <Spinner />;
  if (!topic) return null;

  const linksCount = topic.links.length;
  const childrenCount = children?.length ?? topic.childrenCount;

  return (
    <div className="min-h-screen pb-28">
      {/* Header */}
      <div className="sticky top-0 z-10 bg-[var(--tg-theme-bg-color,#fff)] border-b border-black/5 px-4 py-3 flex items-center gap-2">
        <button onClick={() => navigate(-1)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] shrink-0">
          <ArrowLeft size={18} />
        </button>
        <h1 className="text-sm font-bold truncate flex-1">{topic.title}</h1>

        {/* Links button */}
        <button
          onClick={() => setPanel('links')}
          className="relative w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
        >
          <Link size={15} className="opacity-60" />
          {linksCount > 0 && (
            <span className="absolute -top-1 -right-1 w-4 h-4 text-[9px] font-bold rounded-full bg-[var(--tg-theme-button-color,#2481cc)] text-white flex items-center justify-center">
              {linksCount}
            </span>
          )}
        </button>

        {/* Child topics button */}
        <button
          onClick={() => setPanel('children')}
          className="relative w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
        >
          <FolderPlus size={15} className="opacity-60" />
          {childrenCount > 0 && (
            <span className="absolute -top-1 -right-1 w-4 h-4 text-[9px] font-bold rounded-full bg-[var(--tg-theme-button-color,#2481cc)] text-white flex items-center justify-center">
              {childrenCount}
            </span>
          )}
        </button>

        <button onClick={() => exportApi.exportTopic(id!, topic.title)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
          <Download size={15} className="opacity-60" />
        </button>
        <button onClick={() => navigate(`/topics/${id}/edit`)} className="w-9 h-9 flex items-center justify-center rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
          <Edit size={15} className="opacity-60" />
        </button>
        <button onClick={() => deleteTopic.mutate()} className="w-9 h-9 flex items-center justify-center rounded-xl bg-red-50">
          <Trash2 size={15} className="text-red-400" />
        </button>
      </div>

      <div className="px-4 pt-4">
        {topic.summary && (
          <div className="mb-5 p-3 rounded-2xl bg-[var(--tg-theme-button-color,#2481cc)]/10 border border-[var(--tg-theme-button-color,#2481cc)]/20">
            <p className="text-sm text-[var(--tg-theme-link-color,#2481cc)] leading-relaxed">💡 {topic.summary}</p>
          </div>
        )}

        {topic.content ? (
          <div data-color-mode="light" className="wmde-markdown-var">
            <MDEditor.Markdown source={topic.content} style={{ background: 'transparent', fontSize: 14, lineHeight: 1.7 }} />
          </div>
        ) : (
          <div className="text-center py-12 opacity-40">
            <p className="text-sm mb-2">Содержимое пока не добавлено</p>
            <button onClick={() => navigate(`/topics/${id}/edit`)} className="text-sm text-[var(--tg-theme-link-color,#2481cc)] font-medium opacity-100">
              Открыть редактор
            </button>
          </div>
        )}
      </div>

      {/* Links panel */}
      <Modal
        open={panel === 'links'}
        title={`Ссылки (${linksCount})`}
        onClose={() => setPanel(null)}
        actions={null}
      >
        <div className="flex flex-col gap-2 mb-4">
          {topic.links.length === 0 && (
            <p className="text-sm opacity-40 text-center py-2">Нет прикреплённых ссылок</p>
          )}
          {topic.links.map(link => (
            <div key={link.id} className="flex items-center gap-2 p-3 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
              <ExternalLink size={14} className="text-[var(--tg-theme-link-color,#2481cc)] shrink-0" />
              <a href={link.url} target="_blank" rel="noopener noreferrer" className="flex-1 text-sm text-[var(--tg-theme-link-color,#2481cc)] truncate font-medium">
                {link.title}
              </a>
              <button onClick={() => deleteLink.mutate(link.id)} className="w-6 h-6 flex items-center justify-center rounded-lg shrink-0">
                <X size={12} className="text-red-400" />
              </button>
            </div>
          ))}
        </div>
        <div className="border-t border-black/5 pt-4 flex flex-col gap-2">
          <Input placeholder="Название" value={linkForm.title} onChange={e => setLinkForm(f => ({ ...f, title: e.target.value }))} />
          <Input placeholder="https://..." value={linkForm.url} onChange={e => setLinkForm(f => ({ ...f, url: e.target.value }))} />
          <Button fullWidth loading={addLink.isPending} onClick={() => addLink.mutate()}>
            <Plus size={14} className="mr-1 inline" /> Добавить ссылку
          </Button>
        </div>
      </Modal>

      {/* Child topics panel */}
      <Modal
        open={panel === 'children'}
        title="Подтемы"
        onClose={() => setPanel(null)}
        actions={null}
      >
        <div className="flex flex-col gap-2 mb-4">
          {(!children || children.length === 0) && (
            <p className="text-sm opacity-40 text-center py-2">Нет вложенных тем</p>
          )}
          {children?.map(c => (
            <button
              key={c.id}
              onClick={() => { setPanel(null); navigate(`/topics/${c.id}`); }}
              className="flex items-center gap-2 p-3 rounded-xl bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] text-left w-full"
            >
              <span className="flex-1 text-sm font-medium truncate">{c.title}</span>
              <span className="text-xs opacity-40 shrink-0">{c.childrenCount > 0 ? `${c.childrenCount} подтем` : ''}</span>
            </button>
          ))}
        </div>
        <div className="border-t border-black/5 pt-4 flex flex-col gap-2">
          <Input placeholder="Название подтемы" value={childTitle} onChange={e => setChildTitle(e.target.value)} />
          <Button fullWidth loading={createChild.isPending} onClick={() => createChild.mutate()}>
            <Plus size={14} className="mr-1 inline" /> Добавить подтему
          </Button>
        </div>
      </Modal>
    </div>
  );
};
