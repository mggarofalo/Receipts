import { useState, useRef, type ComponentProps } from "react";
import { cn } from "@/lib/utils";
import { formatDecimal, parseCurrencyInput } from "@/lib/format";

interface CurrencyInputProps
  extends Omit<ComponentProps<"input">, "type" | "value" | "onChange"> {
  value: number;
  onChange: (value: number) => void;
  onBlur?: () => void;
  symbol?: string;
}

export function CurrencyInput({
  value,
  onChange,
  onBlur,
  symbol = "$",
  className,
  ...props
}: CurrencyInputProps) {
  const [text, setText] = useState(() =>
    value === 0 ? "" : formatDecimal(value),
  );
  const inputRef = useRef<HTMLInputElement>(null);
  const [focused, setFocused] = useState(false);

  // Track the last value we reported via onChange so we can distinguish
  // external prop changes (form.reset()) from echoed changes (user typing).
  const [lastEmitted, setLastEmitted] = useState(value);

  // Detect external prop changes (e.g. form.reset()) and sync internal text.
  // This is the React-recommended pattern for adjusting state when a prop changes:
  // https://react.dev/learn/you-might-not-need-an-effect#adjusting-some-state-when-a-prop-changes
  const [prevValue, setPrevValue] = useState(value);
  if (value !== prevValue) {
    setPrevValue(value);
    // Only sync text for external changes (value differs from what we last emitted).
    // This avoids formatting partial input while the user is typing (e.g. "1" -> "1.00").
    if (value !== lastEmitted) {
      setText(value === 0 ? "" : formatDecimal(value));
      setLastEmitted(value);
    }
  }

  // When not focused, show empty string for zero so placeholder is visible
  const displayValue = focused ? text : value === 0 ? "" : formatDecimal(value);

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const raw = parseCurrencyInput(e.target.value);
    const parts = raw.split(".");
    if (parts.length > 1 && parts[1].length > 2) return;
    setText(raw);
    const num = parseFloat(raw);
    if (!isNaN(num)) {
      setLastEmitted(num);
      onChange(num);
    } else if (raw === "" || raw === ".") {
      setLastEmitted(0);
      onChange(0);
    }
  }

  function handleFocus() {
    setFocused(true);
    if (value === 0) {
      setText("");
    } else {
      setText(formatDecimal(value));
      setTimeout(() => inputRef.current?.select(), 0);
    }
  }

  function handleBlur() {
    setFocused(false);
    const num = parseFloat(text);
    const final = isNaN(num) ? 0 : num;
    setText(final === 0 ? "" : formatDecimal(final));
    setLastEmitted(final);
    onChange(final);
    onBlur?.();
  }

  function handlePaste(e: React.ClipboardEvent<HTMLInputElement>) {
    e.preventDefault();
    const pasted = e.clipboardData.getData("text");
    const cleaned = parseCurrencyInput(pasted);
    const parts = cleaned.split(".");
    const limited =
      parts.length > 1 ? `${parts[0]}.${parts[1].slice(0, 2)}` : cleaned;
    setText(limited);
    const num = parseFloat(limited);
    const final = isNaN(num) ? 0 : num;
    setLastEmitted(final);
    onChange(final);
  }

  return (
    <div className="relative">
      <span className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground text-sm">
        {symbol}
      </span>
      <input
        ref={inputRef}
        type="text"
        inputMode="decimal"
        autoComplete="off"
        value={displayValue}
        placeholder="0.00"
        onChange={handleChange}
        onFocus={handleFocus}
        onBlur={handleBlur}
        onPaste={handlePaste}
        className={cn(
          "flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive",
          "pl-7 text-right [appearance:textfield] [&::-webkit-inner-spin-button]:appearance-none [&::-webkit-outer-spin-button]:appearance-none",
          className,
        )}
        {...props}
      />
    </div>
  );
}
