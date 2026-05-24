import { ChevronRight, FileText, Folder } from 'lucide-react';
import type { TopicListItem } from '../types';
import { Card } from './ui/Card';

interface Props {
  topic: TopicListItem;
  onClick: () => void;
}

export const TopicCard = ({ topic, onClick }: Props) => (
  <Card onClick={onClick} className="flex items-center gap-3">
    <div className="shrink-0 w-8 h-8 rounded-lg bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] flex items-center justify-center">
      {topic.childrenCount > 0
        ? <Folder size={15} className="text-[var(--tg-theme-link-color,#2481cc)]" />
        : <FileText size={14} className="opacity-40" />
      }
    </div>
    <div className="flex-1 min-w-0">
      <p className="font-medium truncate">{topic.title}</p>
      {topic.summary && <p className="text-xs opacity-50 truncate mt-0.5">{topic.summary}</p>}
      {topic.childrenCount > 0 && (
        <p className="text-xs opacity-40 mt-0.5">{topic.childrenCount} подтем</p>
      )}
    </div>
    <ChevronRight size={16} className="opacity-30 shrink-0" />
  </Card>
);
