import { useQuery } from '@tanstack/react-query';
import { ChevronDown, ChevronRight, FileText, X } from 'lucide-react';
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

  const handleClick = () => {
    if (hasChildren) setExpanded(e => !e);
    if (node.hasContent) { navigate(`/topics/${node.id}`); onNavigate(); }
  };

  return (
    <div>
      <button
        onClick={handleClick}
        style={{ paddingLeft: `${(depth + 1) * 12}px` }}
        className="w-full flex items-center gap-1.5 py-1.5 pr-2 text-left text-sm hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-lg transition-colors"
      >
        <span className="shrink-0 w-4 text-center">
          {hasChildren
            ? (expanded ? <ChevronDown size={12} /> : <ChevronRight size={12} />)
            : <FileText size={11} className="opacity-30" />
          }
        </span>
        <span className="truncate flex-1 opacity-80">{node.title}</span>
      </button>
      {expanded && hasChildren && (
        <div>{node.children.map(c => <TreeNode key={c.id} node={c} depth={depth + 1} onNavigate={onNavigate} />)}</div>
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
  const { data: tree } = useQuery({ queryKey: ['tree'], queryFn: treeApi.getFullTree, enabled: open });

  const toggleSection = (id: string) =>
    setExpandedSections(s => { const n = new Set(s); n.has(id) ? n.delete(id) : n.add(id); return n; });

  if (!open) return null;

  return (
    <>
      <div className="fixed inset-0 z-40 bg-black/30 backdrop-blur-sm" onClick={onClose} />
      <div className="fixed left-0 top-0 bottom-0 z-50 w-72 bg-[var(--tg-theme-bg-color,#fff)] shadow-2xl flex flex-col overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-black/5">
          <h2 className="font-bold text-base">Навигация</h2>
          <button onClick={onClose} className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
            <X size={16} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-2">
          {tree?.map(section => (
            <div key={section.id} className="mb-1">
              <button
                onClick={() => { toggleSection(section.id); navigate(`/sections/${section.id}`); onClose(); }}
                className="w-full flex items-center gap-2 px-3 py-2 text-sm font-semibold hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-xl transition-colors"
              >
                <span>{section.icon}</span>
                <span className="flex-1 text-left truncate">{section.title}</span>
                <span className="opacity-40" onClick={e => { e.stopPropagation(); toggleSection(section.id); }}>
                  {expandedSections.has(section.id) ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                </span>
              </button>

              {expandedSections.has(section.id) && section.subsections.map(sub => (
                <div key={sub.id} className="ml-2">
                  <button
                    onClick={() => { navigate(`/subsections/${sub.id}`); onClose(); }}
                    className="w-full flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-[var(--tg-theme-link-color,#2481cc)] hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-lg transition-colors"
                  >
                    <span className="truncate">📂 {sub.title}</span>
                  </button>
                  {sub.topics.map(t => <TreeNode key={t.id} node={t} depth={0} onNavigate={onClose} />)}
                </div>
              ))}
            </div>
          ))}

          {(!tree || tree.length === 0) && (
            <p className="text-center text-sm opacity-40 py-8">Нет разделов</p>
          )}
        </div>
      </div>
    </>
  );
};
