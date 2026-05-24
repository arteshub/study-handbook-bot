import { useState, useRef, useEffect } from 'react';
import { Send, X } from 'lucide-react';
import { chatApi, type ChatMsg } from '../api/chat';
import MDEditor from '@uiw/react-md-editor';

interface Props {
  topicId: string;
  questionContext: string;
  modelAnswerContext: string;
  onClose: () => void;
}

export const TopicChat = ({ topicId, questionContext, modelAnswerContext, onClose }: Props) => {
  const [history, setHistory] = useState<ChatMsg[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [history, loading]);

  const send = async () => {
    const text = input.trim();
    if (!text || loading) return;
    const userMsg: ChatMsg = { role: 'user', content: text };
    setHistory(h => [...h, userMsg]);
    setInput('');
    setLoading(true);
    try {
      const reply = await chatApi.send({
        topicId,
        questionContext,
        modelAnswerContext,
        history,
        message: text,
      });
      setHistory(h => [...h, { role: 'assistant', content: reply }]);
    } catch {
      setHistory(h => [...h, { role: 'assistant', content: '⚠️ Ошибка. Попробуй ещё раз.' }]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-[var(--tg-theme-bg-color,#fff)]">
      {/* Header */}
      <div className="flex items-center gap-3 px-4 py-3 border-b border-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
        <button onClick={onClose} className="p-1 rounded-lg hover:bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)]">
          <X size={18} />
        </button>
        <div className="flex-1">
          <p className="font-semibold text-sm">AI Наставник</p>
          <p className="text-xs opacity-40 truncate">{questionContext}</p>
        </div>
      </div>

      {/* Messages */}
      <div className="flex-1 overflow-y-auto px-4 py-3 flex flex-col gap-3">
        {history.length === 0 && (
          <div className="flex-1 flex flex-col items-center justify-center text-center gap-2 py-12 opacity-40">
            <span className="text-3xl">💬</span>
            <p className="text-sm">Задай вопрос по теме — разберём глубже</p>
          </div>
        )}
        {history.map((msg, i) => (
          <div key={i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div
              className={`max-w-[85%] px-3 py-2 rounded-2xl text-sm leading-relaxed ${
                msg.role === 'user'
                  ? 'bg-[var(--tg-theme-button-color,#2481cc)] text-white rounded-br-sm'
                  : 'bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-bl-sm'
              }`}
            >
              {msg.role === 'assistant' ? (
                <div data-color-mode="light" className="wmde-markdown-var">
                  <MDEditor.Markdown
                    source={msg.content}
                    style={{ background: 'transparent', fontSize: 13, lineHeight: 1.6 }}
                  />
                </div>
              ) : (
                msg.content
              )}
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div className="bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] px-4 py-2 rounded-2xl rounded-bl-sm">
              <span className="flex gap-1">
                <span className="w-1.5 h-1.5 rounded-full bg-current opacity-40 animate-bounce" style={{ animationDelay: '0ms' }} />
                <span className="w-1.5 h-1.5 rounded-full bg-current opacity-40 animate-bounce" style={{ animationDelay: '150ms' }} />
                <span className="w-1.5 h-1.5 rounded-full bg-current opacity-40 animate-bounce" style={{ animationDelay: '300ms' }} />
              </span>
            </div>
          </div>
        )}
        <div ref={bottomRef} />
      </div>

      {/* Input */}
      <div className="px-3 py-3 border-t border-[var(--tg-theme-secondary-bg-color,#f1f1f1)] flex gap-2 items-end">
        <textarea
          value={input}
          onChange={e => setInput(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); send(); } }}
          placeholder="Спроси что-нибудь..."
          rows={1}
          className="flex-1 bg-[var(--tg-theme-secondary-bg-color,#f1f1f1)] rounded-2xl px-4 py-2.5 text-sm outline-none resize-none max-h-28 overflow-y-auto"
          style={{ lineHeight: '1.5' }}
        />
        <button
          onClick={send}
          disabled={!input.trim() || loading}
          className="w-9 h-9 rounded-full bg-[var(--tg-theme-button-color,#2481cc)] text-white flex items-center justify-center shrink-0 disabled:opacity-30 transition-opacity"
        >
          <Send size={15} />
        </button>
      </div>
    </div>
  );
};
