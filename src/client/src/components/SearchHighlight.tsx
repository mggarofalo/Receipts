type IndexRange = readonly [number, number];

interface SearchHighlightProps {
  text: string;
  indices?: readonly IndexRange[];
  highlightClassName?: string;
}

function mergeRanges(indices: readonly IndexRange[]): IndexRange[] {
  if (indices.length === 0) return [];
  const sorted = [...indices].sort((a, b) => a[0] - b[0]);
  const merged: [number, number][] = [[sorted[0][0], sorted[0][1]]];
  for (let i = 1; i < sorted.length; i++) {
    const last = merged[merged.length - 1];
    if (sorted[i][0] <= last[1] + 1) {
      last[1] = Math.max(last[1], sorted[i][1]);
    } else {
      merged.push([sorted[i][0], sorted[i][1]]);
    }
  }
  return merged;
}

export function SearchHighlight({
  text,
  indices,
  highlightClassName = "bg-yellow-200 dark:bg-yellow-800 rounded-sm px-0.5",
}: SearchHighlightProps) {
  if (!indices || indices.length === 0) {
    return <>{text}</>;
  }

  const merged = mergeRanges(indices);
  const parts: React.ReactNode[] = [];
  let lastEnd = 0;

  for (const [start, end] of merged) {
    if (start > lastEnd) {
      parts.push(text.slice(lastEnd, start));
    }
    parts.push(
      <mark key={start} className={highlightClassName}>
        {text.slice(start, end + 1)}
      </mark>,
    );
    lastEnd = end + 1;
  }

  if (lastEnd < text.length) {
    parts.push(text.slice(lastEnd));
  }

  return <>{parts}</>;
}
