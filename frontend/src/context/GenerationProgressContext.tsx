import { HubConnectionBuilder } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { createContext, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { youtubeApi } from '../api/youtube';
import { WebApp } from '../lib/telegram';

export interface GenerationProgress {
  topicId: string;
  videoTitle: string;
  progress: number;
  isCompleted: boolean;
  error?: string;
}

interface ContextValue {
  jobs: Map<string, GenerationProgress>;
  addJob: (job: GenerationProgress) => void;
}

const GenerationProgressContext = createContext<ContextValue>({ jobs: new Map(), addJob: () => {} });

export const useGenerationProgress = () => useContext(GenerationProgressContext);

export const GenerationProgressProvider = ({ children }: { children: React.ReactNode }) => {
  const [jobs, setJobs] = useState<Map<string, GenerationProgress>>(new Map());
  const qc = useQueryClient();
  const qcRef = useRef(qc);
  useEffect(() => { qcRef.current = qc; });

  useEffect(() => {
    youtubeApi.getActiveJobs().then(activeJobs => {
      if (!activeJobs.length) return;
      setJobs(prev => {
        const next = new Map(prev);
        for (const job of activeJobs) {
          if (!next.has(job.topicId))
            next.set(job.topicId, { ...job, isCompleted: false });
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
      setJobs(prev => new Map(prev).set(data.topicId, data));
      if (data.isCompleted) {
        qcRef.current.invalidateQueries({ queryKey: ['topics'] });
        setTimeout(() => {
          setJobs(prev => { const next = new Map(prev); next.delete(data.topicId); return next; });
        }, 3000);
      }
    });

    connection.start().catch(() => {});
    return () => { connection.stop(); };
  }, []);

  const addJob = useCallback((job: GenerationProgress) => {
    setJobs(prev => new Map(prev).set(job.topicId, job));
  }, []);

  return (
    <GenerationProgressContext.Provider value={{ jobs, addJob }}>
      {children}
    </GenerationProgressContext.Provider>
  );
};
