import { useCallback, useEffect, useRef, useState } from 'react';

export function useContentSearch(containerRef: React.RefObject<HTMLDivElement | null>) {
  const [query, setQuery] = useState('');
  const [matchCount, setMatchCount] = useState(0);
  const [currentIndex, setCurrentIndex] = useState(-1);

  const originalHtml = useRef<string | null>(null);
  const matchEls = useRef<HTMLElement[]>([]);
  const curIdxRef = useRef(-1);

  const paintCurrent = (idx: number, active: boolean) => {
    const el = matchEls.current[idx];
    if (el) el.style.background = active ? '#ff9500' : '#ffeb3b';
  };

  const goTo = useCallback((idx: number) => {
    paintCurrent(curIdxRef.current, false);
    curIdxRef.current = idx;
    setCurrentIndex(idx);
    paintCurrent(idx, true);
    matchEls.current[idx]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }, []);

  const applyHighlight = useCallback((q: string) => {
    const el = containerRef.current;
    if (!el) return;

    // Save original HTML before first highlight
    if (originalHtml.current === null) {
      originalHtml.current = el.innerHTML;
    }

    // Restore original
    el.innerHTML = originalHtml.current;
    matchEls.current = [];
    curIdxRef.current = -1;

    if (!q.trim()) {
      setMatchCount(0);
      setCurrentIndex(-1);
      return;
    }

    const escaped = q.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    const regex = new RegExp(`(${escaped})`, 'gi');

    // Collect text nodes that contain the query
    const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT);
    const textNodes: Text[] = [];
    let n: Node | null;
    while ((n = walker.nextNode())) {
      regex.lastIndex = 0;
      if (regex.test(n.textContent ?? '')) textNodes.push(n as Text);
    }

    // Wrap matches in <mark>
    for (const tn of textNodes) {
      const text = tn.textContent ?? '';
      regex.lastIndex = 0;
      const span = document.createElement('span');
      span.innerHTML = text.replace(
        regex,
        match => `<mark style="background:#ffeb3b;color:inherit;border-radius:2px;padding:0 1px">${match}</mark>`
      );
      tn.parentNode?.replaceChild(span, tn);
    }

    matchEls.current = Array.from(el.querySelectorAll('mark')) as HTMLElement[];
    setMatchCount(matchEls.current.length);

    if (matchEls.current.length > 0) {
      goTo(0);
    } else {
      setCurrentIndex(-1);
    }
  }, [containerRef, goTo]);

  // Re-highlight when query changes
  useEffect(() => {
    applyHighlight(query);
  }, [query]); // eslint-disable-line react-hooks/exhaustive-deps

  const next = useCallback(() => {
    const count = matchEls.current.length;
    if (count === 0) return;
    goTo((curIdxRef.current + 1) % count);
  }, [goTo]);

  const prev = useCallback(() => {
    const count = matchEls.current.length;
    if (count === 0) return;
    goTo((curIdxRef.current - 1 + count) % count);
  }, [goTo]);

  const clear = useCallback(() => {
    const el = containerRef.current;
    if (el && originalHtml.current !== null) {
      el.innerHTML = originalHtml.current;
      originalHtml.current = null;
    }
    matchEls.current = [];
    curIdxRef.current = -1;
    setQuery('');
    setMatchCount(0);
    setCurrentIndex(-1);
  }, [containerRef]);

  return { query, setQuery, matchCount, currentIndex, next, prev, clear };
}
