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
  const [editing, setEditing] = useState<string | null>(null);

  const flaggedIds = useMemo(
    () => lines.filter((l) => l.flagged).map((l) => l.id),
    [lines],
  );

  useEffect(() => {
    if (!open) {
      setResolved(new Set());
      setEditing(null);
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

  useEffect(() => {
    if (!open) return;
    function handler(e: KeyboardEvent) {
      if (e.key === "Escape") {
        if (editing !== null) {
          setEditing(null);
        } else {
          closeRef.current();
        }
        e.preventDefault();
        return;
      }
      if (editing !== null) return;
      if (flaggedIds.length === 0) return;
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
      } else if (e.key === "e") {
        setEditing(flaggedIds[focus]);
        e.preventDefault();
      } else if (e.key === "r") {
        const id = flaggedIds[focus];
        setResolved((s) => new Set(s).add(id));
        e.preventDefault();
      }
    }
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [open, focus, editing, flaggedIds]);

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
      onClick={onClose}
    >
      <aside
        className="recon-sheet"
        role="dialog"
        aria-modal="true"
        aria-labelledby="recon-title"
        onClick={(e) => e.stopPropagation()}
      >
        <header className="recon-head">
          <div>
            <div className="recon-title" id="recon-title">
              Reconcile receipt
            </div>
            <div className="recon-sub">
              REC-{receiptId.slice(0, 8).toUpperCase()} · {receiptLabel} ·{" "}
              {receiptDate} · {flaggedIds.length} flagged{" "}
              {flaggedIds.length === 1 ? "line" : "lines"}
            </div>
          </div>
          <button
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
                  role={line.flagged ? "button" : undefined}
                  tabIndex={line.flagged ? 0 : undefined}
                  onClick={() => {
                    if (!line.flagged) return;
                    setFocus(flaggedIds.indexOf(line.id));
                  }}
                  onKeyDown={(e) => {
                    if (!line.flagged) return;
                    if (e.key === "Enter" || e.key === " ") {
                      setFocus(flaggedIds.indexOf(line.id));
                      e.preventDefault();
                    }
                  }}
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
                    <span
                      className="actions"
                      onClick={(e) => e.stopPropagation()}
                      onKeyDown={(e) => e.stopPropagation()}
                    >
                      <button
                        type="button"
                        className="accept"
                        title="Mark resolved (a)"
                        aria-label={`Mark ${line.label} resolved`}
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
          <span className="hint">
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
