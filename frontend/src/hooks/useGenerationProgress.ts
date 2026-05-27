import { HubConnectionBuilder } from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';
import { youtubeApi } from '../api/youtube';
import { WebApp } from '../lib/telegram';

export interface GenerationProgress {
  topicId: string;
  videoTitle: string;
  progress: number;
  isCompleted: boolean;
  error?: string;
}

export const useGenerationProgress = (onJobCompleted: (topicId: string) => void) => {
  const [jobs, setJobs] = useState<Map<string, GenerationProgress>>(new Map());
  const onJobCompletedRef = useRef(onJobCompleted);
  useEffect(() => { onJobCompletedRef.current = onJobCompleted; });

  useEffect(() => {
    youtubeApi.getActiveJobs().then(activeJobs => {
      if (activeJobs.length === 0) return;
      setJobs(prev => {
        const next = new Map(prev);
        for (const job of activeJobs) {
          if (!next.has(job.topicId)) {
            next.set(job.topicId, { topicId: job.topicId, videoTitle: job.videoTitle, progress: job.progress, isCompleted: false });
          }
        }
        return next;
      });
    }).catch(() => {});

    const userId = WebApp.initDataUnsafe?.user?.id ?? 12345;
    const connection = new HubConnectionBuilder()
      .withUrl(`/hubs/reading?userId=${userId}`)
      .withAutomaticReconnect()
      .build();

    connection.on('YoutubeGenerationProgress', (data: GenerationProgress) => {
      setJobs(prev => {
        const next = new Map(prev);
        next.set(data.topicId, data);
        return next;
      });
      if (data.isCompleted) {
        onJobCompletedRef.current(data.topicId);
        setTimeout(() => {
          setJobs(prev => {
            const next = new Map(prev);
            next.delete(data.topicId);
            return next;
          });
        }, 3000);
      }
    });

    connection.start().catch(() => {});
    return () => { connection.stop(); };
  }, []);

  const addJob = (job: GenerationProgress) => {
    setJobs(prev => new Map(prev).set(job.topicId, job));
  };

  return { jobs, addJob };
};
