import { apiClient } from './client';

const downloadPdf = (data: Blob, name: string) => {
  const url = URL.createObjectURL(data);
  const a = document.createElement('a');
  a.href = url;
  a.download = name;
  a.click();
  URL.revokeObjectURL(url);
};

export const exportApi = {
  exportSection: async (sectionId: string, name: string) => {
    const res = await apiClient.get(`/export/sections/${sectionId}`, { responseType: 'blob' });
    downloadPdf(res.data, `${name}.pdf`);
  },
  exportTopic: async (topicId: string, name: string) => {
    const res = await apiClient.get(`/export/topics/${topicId}`, { responseType: 'blob' });
    downloadPdf(res.data, `${name}.pdf`);
  },
  exportAll: async () => {
    const res = await apiClient.get('/export/all', { responseType: 'blob' });
    downloadPdf(res.data, 'handbook.pdf');
  },
};
