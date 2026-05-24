import type { Topic, TopicListItem } from '../types';
import { apiClient } from './client';

export const topicsApi = {
  getBySubsectionId: (subsectionId: string) =>
    apiClient.get<TopicListItem[]>(`/subsections/${subsectionId}/topics`).then(r => r.data),
  getById: (id: string) => apiClient.get<Topic>(`/topics/${id}`).then(r => r.data),
  create: (subsectionId: string, data: { title: string; content: string; summary?: string }) =>
    apiClient.post<{ id: string }>(`/subsections/${subsectionId}/topics`, data).then(r => r.data),
  update: (id: string, data: { title: string; content: string; summary?: string }) =>
    apiClient.put(`/topics/${id}`, data),
  delete: (id: string) => apiClient.delete(`/topics/${id}`),
};
