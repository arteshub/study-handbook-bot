import { ChevronRight } from 'lucide-react';
import type { TopicListItem } from '../types';
import { Card } from './ui/Card';

interface Props {
  topic: TopicListItem;
  onClick: () => void;
}

export const TopicCard = ({ topic, onClick }: Props) => (
  <Card onClick={onClick} className="flex items-center gap-3">
    <div className="flex-1 min-w-0">
      <p className="font-medium truncate">{topic.title}</p>
      {topic.summary && <p className="text-sm opacity-60 truncate mt-0.5">{topic.summary}</p>}
      <p className="text-xs opacity-40 mt-1">
        {new Date(topic.updatedAt).toLocaleDateString('ru-RU')}
      </p>
    </div>
    <ChevronRight size={18} className="opacity-30 shrink-0" />
  </Card>
);
