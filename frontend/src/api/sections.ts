import type { Section } from '../types';
import { apiClient } from './client';

export const sectionsApi = {
  getAll: () => apiClient.get<Section[]>('/sections').then(r => r.data),
  getById: (id: string) => apiClient.get<Section>(`/sections/${id}`).then(r => r.data),
  create: (data: { title: string; description?: string; icon?: string }) =>
    apiClient.post<{ id: string }>('/sections', data).then(r => r.data),
  update: (id: string, data: { title: string; description?: string; icon?: string }) =>
    apiClient.put(`/sections/${id}`, data),
  delete: (id: string) => apiClient.delete(`/sections/${id}`),
};
