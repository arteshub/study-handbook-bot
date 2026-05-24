import { ChevronRight, FileText, Layers } from 'lucide-react';
import type { Section } from '../types';
import { Card } from './ui/Card';

interface Props {
  section: Section;
  onClick: () => void;
}

export const SectionCard = ({ section, onClick }: Props) => (
  <Card onClick={onClick} className="flex items-center gap-4">
    <div className="text-3xl w-12 h-12 flex items-center justify-center rounded-xl bg-[var(--tg-theme-bg-color,#fff)]">
      {section.icon}
    </div>
    <div className="flex-1 min-w-0">
      <p className="font-semibold text-base truncate">{section.title}</p>
      {section.description && (
        <p className="text-sm opacity-60 truncate mt-0.5">{section.description}</p>
      )}
      <div className="flex items-center gap-3 mt-1.5 text-xs opacity-50">
        <span className="flex items-center gap-1"><Layers size={11} />{section.subsectionCount} разд.</span>
        <span className="flex items-center gap-1"><FileText size={11} />{section.topicCount} тем</span>
      </div>
    </div>
    <ChevronRight size={18} className="opacity-30 shrink-0" />
  </Card>
);
