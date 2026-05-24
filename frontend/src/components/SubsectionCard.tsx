import { ChevronRight, FileText } from 'lucide-react';
import type { Subsection } from '../types';
import { Card } from './ui/Card';

interface Props {
  subsection: Subsection;
  onClick: () => void;
}

export const SubsectionCard = ({ subsection, onClick }: Props) => (
  <Card onClick={onClick} className="flex items-center gap-3">
    <div className="flex-1 min-w-0">
      <p className="font-semibold truncate">{subsection.title}</p>
      {subsection.description && (
        <p className="text-sm opacity-60 truncate mt-0.5">{subsection.description}</p>
      )}
      <p className="text-xs opacity-40 mt-1 flex items-center gap-1">
        <FileText size={11} />{subsection.topicCount} тем
      </p>
    </div>
    <ChevronRight size={18} className="opacity-30 shrink-0" />
  </Card>
);
