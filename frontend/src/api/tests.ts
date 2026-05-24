import type { TestAnswerResult, TestMode, TestQuestion, TestSession } from '../types';
import { apiClient } from './client';

export const testsApi = {
  startSession: (data: {
    mode: TestMode;
    sectionId?: string;
    subsectionId?: string;
    topicId?: string;
    questionsPerTopic?: number;
  }) => apiClient.post<TestSession>('/tests/sessions', data).then(r => r.data),

  getSession: (id: string) =>
    apiClient.get<TestSession>(`/tests/sessions/${id}`).then(r => r.data),

  getNextQuestion: (sessionId: string) =>
    apiClient.get<TestQuestion | null>(`/tests/sessions/${sessionId}/next`).then(r => r.data),

  submitAnswer: (sessionId: string, data: {
    resultId: string;
    userAnswer?: string;
    selfMarkedCorrect?: boolean;
  }) => apiClient.post<TestAnswerResult>(`/tests/sessions/${sessionId}/answers`, data).then(r => r.data),

  complete: (sessionId: string) =>
    apiClient.post<TestSession>(`/tests/sessions/${sessionId}/complete`).then(r => r.data),

  getHistory: () =>
    apiClient.get<TestSession[]>('/tests/history').then(r => r.data),
};
