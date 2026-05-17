interface Phase {
  label: string;
  key: string;
}

const PHASES: Phase[] = [
  { key: "scan", label: "Scan" },
  { key: "review", label: "Review" },
];

interface ScanPhaseIndicatorProps {
  currentPhase: "scan" | "review";
}

export function ScanPhaseIndicator({ currentPhase }: ScanPhaseIndicatorProps) {
  return (
    <nav aria-label="Scan receipt steps">
      <ol className="flex items-center gap-4 text-sm font-medium">
        {PHASES.map((phase, index) => {
          const isCurrent = phase.key === currentPhase;
          const isPast =
            PHASES.findIndex((p) => p.key === currentPhase) > index;

          return (
            <li
              key={phase.key}
              className="flex items-center gap-2"
              aria-current={isCurrent ? "step" : undefined}
            >
              <span
                className={[
                  "flex h-6 w-6 items-center justify-center rounded-full text-xs font-bold",
                  isCurrent
                    ? "bg-primary text-primary-foreground"
                    : isPast
                      ? "bg-primary/20 text-primary"
                      : "bg-muted text-muted-foreground",
                ].join(" ")}
                aria-hidden="true"
              >
                {index + 1}
              </span>
              <span
                className={
                  isCurrent
                    ? "text-foreground"
                    : isPast
                      ? "text-primary"
                      : "text-muted-foreground"
                }
              >
                {phase.label}
              </span>
              {index < PHASES.length - 1 && (
                <span className="ml-2 text-muted-foreground" aria-hidden="true">
                  &rsaquo;
                </span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
