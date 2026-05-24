import type { Topic, TopicLink, TopicListItem } from '../types';
import { apiClient } from './client';

export const topicsApi = {
  getBySubsectionId: (subsectionId: string, parentTopicId?: string) =>
    apiClient.get<TopicListItem[]>(`/subsections/${subsectionId}/topics`, {
      params: parentTopicId ? { parentTopicId } : undefined,
    }).then(r => r.data),

  getById: (id: string) => apiClient.get<Topic>(`/topics/${id}`).then(r => r.data),

  create: (subsectionId: string, data: { title: string; content: string; summary?: string; parentTopicId?: string }) =>
    apiClient.post<{ id: string }>(`/subsections/${subsectionId}/topics`, data).then(r => r.data),

  update: (id: string, data: { title: string; content: string; summary?: string }) =>
    apiClient.put(`/topics/${id}`, data),

  delete: (id: string) => apiClient.delete(`/topics/${id}`),

  addLink: (topicId: string, data: { title: string; url: string }) =>
    apiClient.post<TopicLink>(`/topics/${topicId}/links`, data).then(r => r.data),

  deleteLink: (topicId: string, linkId: string) =>
    apiClient.delete(`/topics/${topicId}/links/${linkId}`),
};
