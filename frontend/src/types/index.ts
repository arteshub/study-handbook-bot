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
  title: string;
  content: string;
  summary?: string;
  order: number;
  createdAt: string;
  updatedAt: string;
}

export interface TopicListItem {
  id: string;
  subsectionId: string;
  title: string;
  summary?: string;
  order: number;
  updatedAt: string;
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
