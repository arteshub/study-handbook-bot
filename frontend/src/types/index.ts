export interface Section {
  id: string;
  title: string;
  description?: string;
  icon: string;
  order: number;
  subsectionCount: number;
  topicCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Subsection {
  id: string;
  sectionId: string;
  title: string;
  description?: string;
  order: number;
  topicCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface Topic {
  id: string;
  subsectionId: string;
  parentTopicId?: string;
  title: string;
  content: string;
  summary?: string;
  order: number;
  childrenCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface TopicListItem {
  id: string;
  subsectionId: string;
  parentTopicId?: string;
  title: string;
  summary?: string;
  order: number;
  childrenCount: number;
  updatedAt: string;
}

export interface TopicTreeNode {
  id: string;
  title: string;
  summary?: string;
  hasContent: boolean;
  order: number;
  children: TopicTreeNode[];
}

export interface SubsectionTree {
  id: string;
  title: string;
  topics: TopicTreeNode[];
}

export interface SectionTree {
  id: string;
  title: string;
  icon: string;
  subsections: SubsectionTree[];
}

export type TestMode = 'AI' | 'Self';

export interface TestSession {
  id: string;
  mode: TestMode;
  totalQuestions: number;
  correctAnswers: number;
  answeredCount: number;
  isCompleted: boolean;
  startedAt: string;
  completedAt?: string;
}

export interface TestQuestion {
  resultId: string;
  topicId: string;
  topicTitle: string;
  question: string;
  questionNumber: number;
  totalQuestions: number;
}

export interface TestAnswerResult {
  isCorrect: boolean;
  correctAnswer: string;
  aiFeedback?: string;
  correctAnswers: number;
  totalAnswered: number;
}
