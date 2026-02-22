interface FieldDiffProps {
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
}

function formatValue(value: string | null): string {
  if (value === null || value === "") return "(empty)";
  return value;
}

export function FieldDiff({ fieldName, oldValue, newValue }: FieldDiffProps) {
  const isCreate = oldValue === null && newValue !== null;
  const isDelete = oldValue !== null && newValue === null;

  return (
    <div className="flex items-start gap-2 text-sm py-1">
      <span className="min-w-[120px] font-medium text-muted-foreground shrink-0">
        {fieldName}
      </span>
      <div className="flex items-center gap-2 flex-wrap min-w-0">
        {isCreate ? (
          <span className="rounded bg-green-100 px-1.5 py-0.5 text-green-800 dark:bg-green-900/30 dark:text-green-400">
            {formatValue(newValue)}
          </span>
        ) : isDelete ? (
          <span className="rounded bg-red-100 px-1.5 py-0.5 text-red-800 line-through dark:bg-red-900/30 dark:text-red-400">
            {formatValue(oldValue)}
          </span>
        ) : (
          <>
            <span className="rounded bg-red-100 px-1.5 py-0.5 text-red-800 dark:bg-red-900/30 dark:text-red-400">
              {formatValue(oldValue)}
            </span>
            <span className="text-muted-foreground">&rarr;</span>
            <span className="rounded bg-green-100 px-1.5 py-0.5 text-green-800 dark:bg-green-900/30 dark:text-green-400">
              {formatValue(newValue)}
            </span>
          </>
        )}
      </div>
    </div>
  );
}
