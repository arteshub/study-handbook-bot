import type { SectionTree } from '../types';
import { apiClient } from './client';

export const treeApi = {
  getFullTree: () => apiClient.get<SectionTree[]>('/tree').then(r => r.data),
};
