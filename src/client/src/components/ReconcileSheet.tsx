import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Icon, Kbd } from "@/components/primitives";
import { formatCurrency } from "@/lib/format";

export type ReconcilePath = "receipt" | "transactions" | "balance";

export interface ReconcileLine {
  id: string;
  kind: "item" | "adjustment";
  label: string;
  qty: string;
  amount: number;
  flagged: boolean;
  reason?: string;
}

export interface ReconcileSheetProps {
  open: boolean;
  onClose: () => void;
  onResolve?: (decision: { path: ReconcilePath; resolvedIds: string[] }) => void;
  receiptId: string;
  receiptLabel: string;
  receiptDate: string;
  receiptTotal: number;
  transactionsTotal: number;
  lines: ReconcileLine[];
}

export function ReconcileSheet({
  open,
  onClose,
  onResolve,
  receiptId,
  receiptLabel,
  receiptDate,
  receiptTotal,
  transactionsTotal,
  lines,
}: ReconcileSheetProps) {
  const [path, setPath] = useState<ReconcilePath>("balance");
  const [focus, setFocus] = useState(0);
  const [resolved, setResolved] = useState<Set<string>>(new Set());

  const flaggedIds = useMemo(
    () => lines.filter((l) => l.flagged).map((l) => l.id),
    [lines],
  );

  useEffect(() => {
    if (!open) {
      setResolved(new Set());
      setFocus(0);
      setPath("balance");
    }
  }, [open]);

  const delta = transactionsTotal - receiptTotal;
  const balanced = Math.abs(delta) < 0.005;
  const allResolved =
    flaggedIds.length === 0 || flaggedIds.every((id) => resolved.has(id));

  const closeRef = useRef(onClose);
  closeRef.current = onClose;
  const sheetRef = useRef<HTMLDivElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const triggerRef = useRef<Element | null>(null);

  useEffect(() => {
    if (!open) return;
    triggerRef.current = document.activeElement;
    sheetRef.current?.focus();
    return () => {
      const trigger = triggerRef.current as HTMLElement | null;
      if (trigger && typeof trigger.focus === "function") trigger.focus();
    };
  }, [open]);

  function focusInsideSheet(): boolean {
    const active = document.activeElement;
    return !!sheetRef.current && active != null && sheetRef.current.contains(active);
  }

  function handleOverlayMouseDown(e: React.MouseEvent<HTMLDivElement>) {
    if (e.target === e.currentTarget) {
      // overlay click — close
      onClose();
      return;
    }
  }
  function handleSheetMouseDown() {
    // Keep focus inside the sheet so keyboard shortcuts continue to fire even
    // when the user clicks on non-focusable content (delta bar, text rows).
    if (!focusInsideSheet()) sheetRef.current?.focus();
  }

  function handleSheetKeyDown(e: React.KeyboardEvent<HTMLDivElement>) {
    if (e.key === "Tab") {
      const root = sheetRef.current;
      if (!root) return;
      const focusables = root.querySelectorAll<HTMLElement>(
        'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
      );
      if (focusables.length === 0) return;
      const first = focusables[0];
      const last = focusables[focusables.length - 1];
      const active = document.activeElement as HTMLElement | null;
      if (e.shiftKey && active === first) {
        last.focus();
        e.preventDefault();
      } else if (!e.shiftKey && active === last) {
        first.focus();
        e.preventDefault();
      }
      return;
    }
    if (e.key === "Escape") {
      closeRef.current();
      e.preventDefault();
      return;
    }
    if (flaggedIds.length === 0) return;
    if (!focusInsideSheet()) return;
    if (e.key === "j" || e.key === "ArrowDown") {
      setFocus((f) => (f + 1) % flaggedIds.length);
      e.preventDefault();
    } else if (e.key === "k" || e.key === "ArrowUp") {
      setFocus((f) => (f - 1 + flaggedIds.length) % flaggedIds.length);
      e.preventDefault();
    } else if (e.key === "a") {
      const id = flaggedIds[focus];
      setResolved((s) => new Set(s).add(id));
      e.preventDefault();
    } else if (e.key === "r") {
      const id = flaggedIds[focus];
      setResolved((s) => new Set(s).add(id));
      e.preventDefault();
    }
  }

  const handleResolve = useCallback(() => {
    onResolve?.({ path, resolvedIds: Array.from(resolved) });
    onClose();
  }, [onResolve, onClose, path, resolved]);

  if (!open) return null;

  const activeId = flaggedIds[focus];
  const saveDisabled = path === "balance" && !allResolved;

  const saveLabel =
    path === "receipt"
      ? "Accept receipt total"
      : path === "transactions"
        ? "Accept transactions"
        : "Save balanced";

  return (
    <div
      className="recon-overlay"
      role="presentation"
      onMouseDown={handleOverlayMouseDown}
    >
      <aside
        ref={sheetRef}
        className="recon-sheet"
        role="dialog"
        aria-modal="true"
        aria-labelledby="recon-title"
        aria-describedby="recon-sub"
        tabIndex={-1}
        onMouseDown={handleSheetMouseDown}
        onKeyDown={handleSheetKeyDown}
      >
        <header className="recon-head">
          <div>
            <div className="recon-title" id="recon-title">
              Reconcile receipt
            </div>
            <div className="recon-sub" id="recon-sub">
              REC-{receiptId.slice(0, 8).toUpperCase()} · {receiptLabel} ·{" "}
              {receiptDate} · {flaggedIds.length} flagged{" "}
              {flaggedIds.length === 1 ? "line" : "lines"}
            </div>
          </div>
          <button
            ref={closeButtonRef}
            type="button"
            className="icon-btn"
            onClick={onClose}
            aria-label="Close reconcile sheet"
          >
            <Icon.X />
          </button>
        </header>

        <div className="recon-delta-bar">
          <div>
            <div className="k">Receipt total</div>
            <div className="v">{formatCurrency(receiptTotal)}</div>
          </div>
          <div className="sep" aria-hidden="true">
            {balanced ? "=" : "≠"}
          </div>
          <div>
            <div className="k">Transactions</div>
            <div className="v">{formatCurrency(transactionsTotal)}</div>
          </div>
          <div className="sep" aria-hidden="true">
            Δ
          </div>
          <div>
            <div className="k">Difference</div>
            <div
              className={
                "v " + (balanced ? "pos" : delta > 0 ? "pos" : "neg")
              }
            >
              {balanced
                ? "±0.00"
                : (delta > 0 ? "+" : "−") +
                  formatCurrency(Math.abs(delta)).replace(/^-/, "")}
            </div>
          </div>
        </div>

        <div className="recon-body">
          <div className="recon-section-label">
            {flaggedIds.length === 0
              ? `${lines.length} ${lines.length === 1 ? "line" : "lines"}`
              : `Flagged lines — ${resolved.size} of ${flaggedIds.length} resolved`}
          </div>

          {lines.length === 0 ? (
            <div className="empty" style={{ marginTop: 0 }}>
              <div className="icon-frame">
                <Icon.AlertTriangle />
              </div>
              <h3>Nothing to reconcile</h3>
              <p>This receipt has no items or adjustments yet.</p>
            </div>
          ) : (
            lines.map((line) => {
              const isActive = line.flagged && activeId === line.id;
              const isResolved = resolved.has(line.id);
              const className =
                "recon-line" +
                (line.flagged ? " flagged" : " muted") +
                (isActive ? " focused" : "") +
                (isResolved ? " resolved" : "");
              return (
                <div
                  key={line.id}
                  className={className}
                  aria-current={isActive ? "true" : undefined}
                >
                  <span
                    className={
                      "marker " +
                      (isResolved ? "ok" : line.flagged ? "flag" : "")
                    }
                    aria-hidden="true"
                  >
                    {isResolved ? "✓" : line.flagged ? "!" : "·"}
                  </span>
                  <span>
                    <span className="lbl">{line.label}</span>
                    <span
                      style={{ display: "block" }}
                      className="qty"
                    >
                      {line.qty}
                      {line.reason ? ` · ${line.reason}` : ""}
                    </span>
                  </span>
                  <span
                    className={
                      "amt" + (line.flagged && !isResolved ? " low" : "")
                    }
                  >
                    {formatCurrency(line.amount)}
                  </span>
                  {line.flagged ? (
                    <span className="actions">
                      <button
                        type="button"
                        className="accept"
                        title="Mark resolved (a)"
                        aria-label={`Mark ${line.label} resolved`}
                        onFocus={() =>
                          setFocus(flaggedIds.indexOf(line.id))
                        }
                        onClick={() =>
                          setResolved((s) => new Set(s).add(line.id))
                        }
                      >
                        <Icon.Check />
                      </button>
                      <button
                        type="button"
                        className="reject"
                        title="Dismiss (r)"
                        aria-label={`Dismiss ${line.label}`}
                        onFocus={() =>
                          setFocus(flaggedIds.indexOf(line.id))
                        }
                        onClick={() =>
                          setResolved((s) => new Set(s).add(line.id))
                        }
                      >
                        <Icon.X />
                      </button>
                    </span>
                  ) : (
                    <span aria-hidden="true" />
                  )}
                </div>
              );
            })
          )}

          <div className="recon-paths-prompt">How should we balance?</div>
          <div className="recon-paths">
            <button
              type="button"
              className={"recon-path" + (path === "receipt" ? " active" : "")}
              onClick={() => setPath("receipt")}
              aria-pressed={path === "receipt"}
            >
              <div className="pt">Accept receipt total</div>
              <div className="pd">
                Trust the printed receipt. Flag transactions for follow-up.
              </div>
              <div className="pv">→ {formatCurrency(receiptTotal)}</div>
            </button>
            <button
              type="button"
              className={
                "recon-path" + (path === "transactions" ? " active" : "")
              }
              onClick={() => setPath("transactions")}
              aria-pressed={path === "transactions"}
            >
              <div className="pt">Accept transactions</div>
              <div className="pd">
                Trust the bank side. Receipt may need an adjustment line.
              </div>
              <div className="pv">→ {formatCurrency(transactionsTotal)}</div>
            </button>
            <button
              type="button"
              className={"recon-path" + (path === "balance" ? " active" : "")}
              onClick={() => setPath("balance")}
              aria-pressed={path === "balance"}
            >
              <div className="pt">Edit and balance</div>
              <div className="pd">
                Walk flagged lines, fix, and save when Δ = 0.
              </div>
              <div className="pv">
                Δ{" "}
                {balanced
                  ? "= $0.00"
                  : "= " +
                    (delta > 0 ? "+" : "−") +
                    formatCurrency(Math.abs(delta)).replace(/^-/, "")}
              </div>
            </button>
          </div>
        </div>

        <footer className="recon-foot">
          <span className="sr-only">
            Keyboard shortcuts: J or K to move between flagged lines, A to
            accept, R to dismiss, Escape to close.
          </span>
          <span className="hint" aria-hidden="true">
            <Kbd>J</Kbd> <Kbd>K</Kbd> move · <Kbd>A</Kbd> accept ·{" "}
            <Kbd>R</Kbd> dismiss · <Kbd>esc</Kbd> close
          </span>
          <span style={{ marginLeft: "auto", display: "flex", gap: 8 }}>
            <button type="button" className="btn" onClick={onClose}>
              Cancel
            </button>
            <button
              type="button"
              className="btn primary"
              onClick={handleResolve}
              disabled={saveDisabled}
            >
              <Icon.Check /> {saveLabel}
            </button>
          </span>
        </footer>
      </aside>
    </div>
  );
}
