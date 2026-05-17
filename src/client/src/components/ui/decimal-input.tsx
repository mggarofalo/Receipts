import { useRef, useState, type ComponentProps } from "react";
import { Input } from "@/components/ui/input";

/** Keep only digits and a single decimal point. */
function sanitize(raw: string): string {
  const cleaned = raw.replace(/[^0-9.]/g, "");
  // Collapse multiple decimal points into the first one.
  return cleaned.replace(/(\..*)\./g, "$1");
}

interface DecimalInputProps
  extends Omit<ComponentProps<"input">, "type" | "value" | "onChange"> {
  value: number;
  onChange: (value: number) => void;
}

/**
 * Numeric text input that accepts decimals typed with a leading point
 * (e.g. ".4" → 0.4). Unlike <input type="number">, the browser never
 * discards a partial value like "." while the user is mid-entry.
 */
export function DecimalInput({
  value,
  onChange,
  onFocus,
  onBlur,
  ...props
}: DecimalInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [text, setText] = useState(() => (value ? String(value) : ""));
  const [focused, setFocused] = useState(false);

  // Track the last value we emitted so we can tell external prop changes
  // (e.g. form.reset()) apart from echoes of the user's own typing.
  const [lastEmitted, setLastEmitted] = useState(value);
  const [prevValue, setPrevValue] = useState(value);
  if (value !== prevValue) {
    setPrevValue(value);
    if (value !== lastEmitted) {
      setText(value ? String(value) : "");
      setLastEmitted(value);
    }
  }

  const displayValue = focused ? text : value ? String(value) : "";

  function handleChange(e: React.ChangeEvent<HTMLInputElement>) {
    const sanitized = sanitize(e.target.value);
    setText(sanitized);
    const num = parseFloat(sanitized);
    if (!isNaN(num)) {
      setLastEmitted(num);
      onChange(num);
    } else {
      // "", "." — nothing parseable yet; report 0 so validation can react.
      setLastEmitted(0);
      onChange(0);
    }
  }

  function handleFocus(e: React.FocusEvent<HTMLInputElement>) {
    setFocused(true);
    setText(value ? String(value) : "");
    setTimeout(() => inputRef.current?.select(), 0);
    onFocus?.(e);
  }

  function handleBlur(e: React.FocusEvent<HTMLInputElement>) {
    setFocused(false);
    onBlur?.(e);
  }

  return (
    <Input
      ref={inputRef}
      type="text"
      inputMode="decimal"
      autoComplete="off"
      value={displayValue}
      placeholder="0"
      onChange={handleChange}
      onFocus={handleFocus}
      onBlur={handleBlur}
      {...props}
    />
  );
}
