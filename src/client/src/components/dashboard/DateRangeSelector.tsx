import { useState, useCallback, useMemo } from "react";
import { format, subDays, startOfYear } from "date-fns";
import { CalendarIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Calendar } from "@/components/ui/calendar";
import type { DateRange } from "@/hooks/useDashboard";
import type { DateRange as DayPickerDateRange } from "react-day-picker";

type PresetKey = "7d" | "30d" | "90d" | "ytd" | "all";

interface Preset {
  label: string;
  getRange: () => DateRange;
}

const presets: Record<PresetKey, Preset> = {
  "7d": {
    label: "7 days",
    getRange: () => ({
      startDate: format(subDays(new Date(), 7), "yyyy-MM-dd"),
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
  "30d": {
    label: "30 days",
    getRange: () => ({
      startDate: format(subDays(new Date(), 30), "yyyy-MM-dd"),
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
  "90d": {
    label: "90 days",
    getRange: () => ({
      startDate: format(subDays(new Date(), 90), "yyyy-MM-dd"),
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
  ytd: {
    label: "Year to date",
    getRange: () => ({
      startDate: format(startOfYear(new Date()), "yyyy-MM-dd"),
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
  all: {
    label: "All time",
    getRange: () => ({
      startDate: "2000-01-01",
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
};

interface DateRangeSelectorProps {
  value: DateRange;
  onChange: (range: DateRange) => void;
}

export function DateRangeSelector({ value, onChange }: DateRangeSelectorProps) {
  const [activePreset, setActivePreset] = useState<PresetKey | "custom">("30d");
  const [calendarOpen, setCalendarOpen] = useState(false);

  const handlePreset = useCallback(
    (key: PresetKey) => {
      setActivePreset(key);
      onChange(presets[key].getRange());
    },
    [onChange],
  );

  const handleCalendarSelect = useCallback(
    (range: DayPickerDateRange | undefined) => {
      if (range?.from) {
        setActivePreset("custom");
        onChange({
          startDate: format(range.from, "yyyy-MM-dd"),
          endDate: range.to
            ? format(range.to, "yyyy-MM-dd")
            : format(range.from, "yyyy-MM-dd"),
        });
        if (range.to) {
          setCalendarOpen(false);
        }
      }
    },
    [onChange],
  );

  const calendarSelected = useMemo<DayPickerDateRange | undefined>(() => {
    if (!value.startDate) return undefined;
    return {
      from: new Date(value.startDate + "T00:00:00"),
      to: value.endDate ? new Date(value.endDate + "T00:00:00") : undefined,
    };
  }, [value.startDate, value.endDate]);

  const displayLabel = useMemo(() => {
    if (activePreset !== "custom" && activePreset in presets) {
      return presets[activePreset].label;
    }
    if (value.startDate && value.endDate) {
      return `${value.startDate} – ${value.endDate}`;
    }
    return "Select range";
  }, [activePreset, value.startDate, value.endDate]);

  return (
    <div className="flex items-center gap-2 flex-wrap">
      {(Object.keys(presets) as PresetKey[]).map((key) => (
        <Button
          key={key}
          variant={activePreset === key ? "default" : "outline"}
          size="sm"
          onClick={() => handlePreset(key)}
        >
          {presets[key].label}
        </Button>
      ))}
      <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
        <PopoverTrigger asChild>
          <Button
            variant={activePreset === "custom" ? "default" : "outline"}
            size="sm"
            className="gap-1.5"
          >
            <CalendarIcon className="h-3.5 w-3.5" />
            {activePreset === "custom" ? displayLabel : "Custom"}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="end">
          <Calendar
            mode="range"
            selected={calendarSelected}
            onSelect={handleCalendarSelect}
            numberOfMonths={2}
            disabled={{ after: new Date() }}
          />
        </PopoverContent>
      </Popover>
    </div>
  );
}
