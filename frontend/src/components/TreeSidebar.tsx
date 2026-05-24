import { useQuery } from '@tanstack/react-query';
import { ChevronDown, ChevronRight, FileText, Folder, X } from 'lucide-react';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { treeApi } from '../api/tree';
import type { TopicTreeNode } from '../types';

interface TreeNodeProps {
  node: TopicTreeNode;
  depth?: number;
  onNavigate: () => void;
}

const TreeNode = ({ node, depth = 0, onNavigate }: TreeNodeProps) => {
  const [expanded, setExpanded] = useState(false);
  const navigate = useNavigate();
  const hasChildren = node.children.length > 0;

  const goTo = () => { navigate(`/topics/${node.id}`); onNavigate(); };

  return (
    <div>
      <div
        style={{ paddingLeft: `${(depth + 1) * 12}px` }}
        className="flex items-center gap-1.5 py-1 pr-1"
      >
        <button
          onClick={() => hasChildren ? setExpanded(e => !e) : goTo()}
          className="shrink-0 w-5 h-5 flex items-center justify-center rounded hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
        >
          {hasChildren
            ? (expanded ? <ChevronDown size={12} /> : <ChevronRight size={12} />)
            : <FileText size={11} className="opacity-30" />
          }
        </button>
        {hasChildren && <Folder size={11} className="shrink-0 text-[var(--tg-theme-link-color,#2481cc)] opacity-60" />}
        <button
          onClick={goTo}
          className="flex-1 text-left text-sm truncate opacity-80 py-0.5 hover:opacity-100"
        >
          {node.title}
        </button>
      </div>
      {expanded && hasChildren && (
        <div>
          {node.children.map(c => <TreeNode key={c.id} node={c} depth={depth + 1} onNavigate={onNavigate} />)}
        </div>
      )}
    </div>
  );
};

interface Props {
  open: boolean;
  onClose: () => void;
}

export const TreeSidebar = ({ open, onClose }: Props) => {
  const navigate = useNavigate();
  const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set());

  const { data: tree, isLoading } = useQuery({
    queryKey: ['tree'],
    queryFn: treeApi.getFullTree,
    enabled: open,
    staleTime: 60_000,
  });

  const toggle = (id: string) =>
    setExpandedSections(s => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  const goTo = (path: string) => { navigate(path); onClose(); };

  if (!open) return null;

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed left-0 top-0 bottom-0 z-50 w-72 bg-[var(--tg-theme-bg-color,#fff)] shadow-2xl flex flex-col overflow-hidden">
        <div className="flex items-center justify-between px-4 py-3 border-b border-black/5 shrink-0">
          <h2 className="font-bold text-base">Навигация</h2>
          <button onClick={onClose} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <X size={16} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-2">
          {isLoading && (
            <div className="flex items-center justify-center py-12">
              <div className="w-6 h-6 border-2 border-[var(--tg-theme-secondary-bg-color,#f1f1f1)] border-t-[var(--tg-theme-button-color,#2481cc)] rounded-full animate-spin" />
            </div>
          )}

          {!isLoading && tree?.map(section => (
            <div key={section.id} className="mb-1">
              {/* Section row */}
              <div className="flex items-center gap-1 px-1 py-0.5">
                <button
                  onClick={() => toggle(section.id)}
                  className="shrink-0 w-5 h-5 flex items-center justify-center rounded hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
                >
                  {expandedSections.has(section.id) ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </button>
                <button
                  onClick={() => goTo(`/sections/${section.id}`)}
                  className="flex-1 flex items-center gap-2 px-2 py-1.5 text-sm font-semibold text-left rounded-xl hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] transition-colors"
                >
                  <span>{section.icon}</span>
                  <span className="truncate">{section.title}</span>
                </button>
              </div>

              {expandedSections.has(section.id) && section.subsections.map(sub => (
                <div key={sub.id} className="ml-3">
                  {/* Subsection row */}
                  <div className="flex items-center gap-1 px-1 py-0.5">
                    <button
                      onClick={() => toggle(sub.id)}
                      className="shrink-0 w-5 h-5 flex items-center justify-center rounded hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]"
                    >
                      {expandedSections.has(sub.id) ? <ChevronDown size={12} /> : <ChevronRight size={12} />}
                    </button>
                    <button
                      onClick={() => goTo(`/subsections/${sub.id}`)}
                      className="flex-1 text-left text-sm font-medium text-[var(--tg-theme-link-color,#2481cc)] px-2 py-1 rounded-lg hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] truncate transition-colors"
                    >
                      📂 {sub.title}
                    </button>
                  </div>
                  {expandedSections.has(sub.id) && sub.topics.map(t => (
                    <TreeNode key={t.id} node={t} depth={0} onNavigate={onClose} />
                  ))}
                </div>
              ))}
            </div>
          ))}

          {!isLoading && tree?.length === 0 && (
            <p className="text-center text-sm opacity-40 py-8">Нет разделов</p>
          )}
        </div>
      </div>
    </>
  );
};
