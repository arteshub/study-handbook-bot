import { apiClient } from './client';

export const readingPositionsApi = {
  get: (topicId: string) =>
    apiClient.get<{ scrollRatio: number }>(`/reading-positions/${topicId}`).then(r => r.data),
};
