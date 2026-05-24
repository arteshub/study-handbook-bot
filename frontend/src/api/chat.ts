import { apiClient } from './client';

export interface ChatMsg { role: 'user' | 'assistant'; content: string; }

export const chatApi = {
  send: (data: {
    topicId: string;
    questionContext: string;
    modelAnswerContext: string;
    history: ChatMsg[];
    message: string;
  }) => apiClient.post<{ reply: string }>('/chat', data).then(r => r.data.reply),
};
