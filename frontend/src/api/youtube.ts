import { apiClient } from './client';

export interface YoutubeExtractResult {
  title: string;
  content: string;
}

export const youtubeApi = {
  extract: (url: string) =>
    apiClient.post<YoutubeExtractResult>('/youtube-extractor', JSON.stringify(url), {
      headers: { 'Content-Type': 'application/json' },
      timeout: 600_000,
    }).then(r => r.data),
};
