/// <reference types="node" />
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";
import { describe, it, expect } from "vitest";

// Regression guard for RECEIPTS-567: `text-muted-foreground` must meet WCAG AA
// (4.5:1 for normal text) against every surface it can plausibly render on,
// in both light and dark themes. Bumping `--muted-foreground` lighter will fail
// this test.
//
// Reads index.css from disk (rather than `?raw` import) because the vite
// tailwindcss plugin transforms CSS imports through the vitest pipeline,
// yielding an empty string for `?raw` queries.

const CSS_PATH = resolve(
  dirname(fileURLToPath(import.meta.url)),
  "../index.css",
);
const CSS = readFileSync(CSS_PATH, "utf8");

const WCAG_AA_NORMAL = 4.5;

// Token neighborhoods a small-text caller might reasonably sit on.
// Extend if new surface tokens are introduced.
const BACKGROUND_TOKENS = [
  "background",
  "card",
  "popover",
  "muted",
  "secondary",
  "accent",
  "sidebar",
  "sidebar-accent",
] as const;

type ThemeBlock = ":root" | ".dark";

function extractBlock(css: string, selector: ThemeBlock): string {
  const start = css.indexOf(`${selector} {`);
  if (start === -1) throw new Error(`Missing ${selector} block in index.css`);
  const end = css.indexOf("}", start);
  return css.slice(start, end);
}

function getTokenL(block: string, name: string): number {
  // Match `--name: oklch(<L> 0 0);` — chroma must be 0 for the gray math below
  // to hold. If a token introduces chroma, the test needs a proper OKLCH→sRGB
  // conversion; the current shadcn theme is all-neutral for these tokens.
  const re = new RegExp(
    `--${name}:\\s*oklch\\(\\s*([0-9.]+)\\s+0\\s+0\\s*\\)`,
  );
  const match = block.match(re);
  if (!match) {
    throw new Error(
      `Token --${name} not found or uses non-neutral OKLCH in block`,
    );
  }
  return Number.parseFloat(match[1]);
}

// For neutral grays (chroma=0), OKLCH L cubed equals the WCAG relative
// luminance: oklab.l = lin_R^(1/3), and for r=g=b the WCAG Y reduces to lin_R.
function luminance(oklchL: number): number {
  return oklchL ** 3;
}

function contrast(fgL: number, bgL: number): number {
  const a = luminance(fgL);
  const b = luminance(bgL);
  const [lighter, darker] = a > b ? [a, b] : [b, a];
  return (lighter + 0.05) / (darker + 0.05);
}

describe("muted-foreground contrast meets WCAG AA", () => {
  for (const theme of [":root", ".dark"] as const) {
    describe(theme, () => {
      const block = extractBlock(CSS, theme);
      const fg = getTokenL(block, "muted-foreground");

      for (const bgName of BACKGROUND_TOKENS) {
        it(`on --${bgName}`, () => {
          const bg = getTokenL(block, bgName);
          const ratio = contrast(fg, bg);
          expect(
            ratio,
            `muted-foreground on --${bgName} in ${theme}: ${ratio.toFixed(2)}:1`,
          ).toBeGreaterThanOrEqual(WCAG_AA_NORMAL);
        });
      }
    });
  }
});
