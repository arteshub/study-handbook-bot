import type { Subsection } from '../types';
import { apiClient } from './client';

export const subsectionsApi = {
  getBySectionId: (sectionId: string) =>
    apiClient.get<Subsection[]>(`/sections/${sectionId}/subsections`).then(r => r.data),
  create: (sectionId: string, data: { title: string; description?: string }) =>
    apiClient.post<{ id: string }>(`/sections/${sectionId}/subsections`, data).then(r => r.data),
  update: (id: string, data: { title: string; description?: string }) =>
    apiClient.put(`/subsections/${id}`, data),
  delete: (id: string) => apiClient.delete(`/subsections/${id}`),
};
