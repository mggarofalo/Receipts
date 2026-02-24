import { Search, X } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";

interface FuzzySearchInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  resultCount?: number;
  totalCount?: number;
  className?: string;
  showShortcutHint?: boolean;
}

export function FuzzySearchInput({
  value,
  onChange,
  placeholder = "Search...",
  resultCount,
  totalCount,
  className,
  showShortcutHint = false,
}: FuzzySearchInputProps) {
  return (
    <div className={cn("relative", className)}>
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        placeholder={placeholder}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="pl-9 pr-24"
      />
      <div className="absolute right-3 top-1/2 flex -translate-y-1/2 items-center gap-1.5">
        {value && resultCount !== undefined && totalCount !== undefined && (
          <Badge variant="secondary" className="text-xs">
            {resultCount}/{totalCount}
          </Badge>
        )}
        {value && (
          <button
            onClick={() => onChange("")}
            className="rounded-sm p-0.5 text-muted-foreground hover:text-foreground"
            aria-label="Clear search"
          >
            <X className="h-3.5 w-3.5" />
          </button>
        )}
        {showShortcutHint && !value && (
          <kbd className="pointer-events-none select-none rounded border bg-muted px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground">
            Ctrl+K
          </kbd>
        )}
      </div>
    </div>
  );
}
