import type { FuseResultMatch } from "fuse.js";

type IndexRange = readonly [number, number];

export function getMatchIndices(
  matches: readonly FuseResultMatch[] | undefined,
  key: string,
): readonly IndexRange[] | undefined {
  if (!matches) return undefined;
  const match = matches.find((m) => m.key === key);
  return match?.indices;
}
