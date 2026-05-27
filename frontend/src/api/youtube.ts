import { apiClient } from './client';

export interface YoutubeExtractResult {
  title: string;
  content: string;
}

export interface ActiveGenerationDto {
  topicId: string;
  videoTitle: string;
  progress: number;
}

export const youtubeApi = {
  extract: (url: string) =>
    apiClient.post<YoutubeExtractResult>('/youtube-extractor', JSON.stringify(url), {
      headers: { 'Content-Type': 'application/json' },
      timeout: 600_000,
    }).then(r => r.data),

  startGeneration: (url: string, subsectionId: string) =>
    apiClient.post<{ topicId: string; videoTitle: string }>('/youtube-extractor/start', { url, subsectionId })
      .then(r => r.data),

  getActiveJobs: () =>
    apiClient.get<ActiveGenerationDto[]>('/youtube-extractor/active').then(r => r.data),
};
