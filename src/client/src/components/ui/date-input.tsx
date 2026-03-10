import { useState, useRef, useCallback, type ComponentProps } from "react";
import { format, parse, isValid } from "date-fns";
import { CalendarIcon } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

/**
 * Supported text input formats for parsing user-typed dates.
 * Tried in order; first valid parse wins.
 */
const PARSE_FORMATS = [
  "yyyy-MM-dd",
  "MM/dd/yyyy",
  "M/d/yyyy",
  "MM-dd-yyyy",
  "M-d-yyyy",
  "MMddyyyy",
  "yyyyMMdd",
];

/** Canonical wire format used by the rest of the app. */
const WIRE_FORMAT = "yyyy-MM-dd";

/** Display format shown to the user in the text input. */
const DISPLAY_FORMAT = "MM/dd/yyyy";

function tryParseDate(text: string): Date | null {
  const trimmed = text.trim();
  if (!trimmed) return null;
  for (const fmt of PARSE_FORMATS) {
    const d = parse(trimmed, fmt, new Date());
    if (isValid(d)) return d;
  }
  return null;
}

/** Convert a wire-format value to display text. */
function wireToDisplay(wire: string): string {
  if (!wire) return "";
  const d = tryParseDate(wire);
  return d ? format(d, DISPLAY_FORMAT) : wire;
}

interface DateInputProps extends Omit<
  ComponentProps<"input">,
  "type" | "value" | "onChange" | "onBlur"
> {
  /** Date value in YYYY-MM-DD wire format. */
  value: string;
  /** Called with YYYY-MM-DD wire format string. */
  onChange: (value: string) => void;
  onBlur?: () => void;
  /** Maximum selectable date in YYYY-MM-DD format. */
  max?: string;
}

export function DateInput({
  value,
  onChange,
  onBlur,
  max,
  className,
  disabled,
  ...props
}: DateInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [open, setOpen] = useState(false);
  const [focused, setFocused] = useState(false);
  // Local text tracks what the user is typing while focused
  const [localText, setLocalText] = useState("");

  // When not focused, always derive the display from the controlled prop.
  // When focused, show the user's in-progress typed text.
  const displayValue = focused ? localText : wireToDisplay(value);

  const commitText = useCallback(
    (raw: string) => {
      const d = tryParseDate(raw);
      if (d) {
        onChange(format(d, WIRE_FORMAT));
      } else if (!raw.trim()) {
        onChange("");
      }
      // If text is non-empty but unparseable, keep it so the user can fix it
    },
    [onChange],
  );

  function handleFocus() {
    setFocused(true);
    // Initialize local text from the current wire value
    setLocalText(wireToDisplay(value));
  }

  function handleTextChange(e: React.ChangeEvent<HTMLInputElement>) {
    setLocalText(e.target.value);
    // Eagerly commit if the typed text is a valid date
    const d = tryParseDate(e.target.value);
    if (d) {
      onChange(format(d, WIRE_FORMAT));
    }
  }

  function handleBlur() {
    commitText(localText);
    setFocused(false);
    onBlur?.();
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") {
      commitText(localText);
    }
  }

  function handleCalendarSelect(date: Date | undefined) {
    if (date) {
      const wire = format(date, WIRE_FORMAT);
      onChange(wire);
      // Also update local text in case input is still focused
      setLocalText(format(date, DISPLAY_FORMAT));
    }
    setOpen(false);
    // Return focus to the text input after picking
    setTimeout(() => inputRef.current?.focus(), 0);
  }

  // Convert wire value to Date for the calendar's `selected` prop
  const selectedDate = value ? (tryParseDate(value) ?? undefined) : undefined;

  // Convert max prop to Date for calendar's `disabled` matcher
  const maxDate = max ? (tryParseDate(max) ?? undefined) : undefined;

  return (
    <div className="relative flex items-center">
      <input
        ref={inputRef}
        type="text"
        inputMode="numeric"
        data-slot="input"
        value={displayValue}
        onFocus={handleFocus}
        onChange={handleTextChange}
        onBlur={handleBlur}
        onKeyDown={handleKeyDown}
        placeholder="MM/DD/YYYY"
        disabled={disabled}
        className={cn(
          "file:text-foreground placeholder:text-muted-foreground selection:bg-primary selection:text-primary-foreground dark:bg-input/30 border-input h-9 w-full min-w-0 rounded-md border bg-transparent px-3 py-1 pr-9 text-base shadow-xs transition-[color,box-shadow] outline-none file:inline-flex file:h-7 file:border-0 file:bg-transparent file:text-sm file:font-medium disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm",
          "focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
          "aria-invalid:ring-destructive/20 dark:aria-invalid:ring-destructive/40 aria-invalid:border-destructive",
          className,
        )}
        {...props}
      />
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            disabled={disabled}
            className="absolute right-0 h-9 w-9 rounded-l-none text-muted-foreground hover:text-foreground"
            aria-label="Pick a date"
          >
            <CalendarIcon className="h-4 w-4" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="end">
          <Calendar
            mode="single"
            selected={selectedDate}
            onSelect={handleCalendarSelect}
            defaultMonth={selectedDate}
            disabled={maxDate ? { after: maxDate } : undefined}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
}
