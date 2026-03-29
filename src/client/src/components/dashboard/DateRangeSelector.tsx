import { useState, useCallback, useMemo } from "react";
import { format, subDays, startOfYear } from "date-fns";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { DateRange } from "@/hooks/useDashboard";
import { useDashboardEarliestReceiptYear } from "@/hooks/useDashboard";

type PresetKey = "30d" | "90d" | "ytd" | "year" | "all";

interface Preset {
  label: string;
  getRange: (selectedYear?: number) => DateRange;
}

const presets: Record<PresetKey, Preset> = {
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
    label: "YTD",
    getRange: () => ({
      startDate: format(startOfYear(new Date()), "yyyy-MM-dd"),
      endDate: format(new Date(), "yyyy-MM-dd"),
    }),
  },
  year: {
    label: "Year",
    getRange: (selectedYear?: number) => {
      const y = selectedYear ?? new Date().getFullYear();
      return {
        startDate: `${y}-01-01`,
        endDate: `${y}-12-31`,
      };
    },
  },
  all: {
    label: "All time",
    getRange: () => ({
      startDate: undefined,
      endDate: undefined,
    }),
  },
};

interface DateRangeSelectorProps {
  value: DateRange;
  onChange: (range: DateRange) => void;
}

export function DateRangeSelector({ value, onChange }: DateRangeSelectorProps) {
  const [activePreset, setActivePreset] = useState<PresetKey>("30d");
  const [selectedYear, setSelectedYear] = useState<number>(
    new Date().getFullYear(),
  );
  const { data: earliestYearData } = useDashboardEarliestReceiptYear();

  const availableYears = useMemo(() => {
    const earliest = earliestYearData?.year ?? new Date().getFullYear();
    const current = new Date().getFullYear();
    const years: number[] = [];
    for (let y = current; y >= earliest; y--) {
      years.push(y);
    }
    return years;
  }, [earliestYearData?.year]);

  const handlePreset = useCallback(
    (key: PresetKey) => {
      setActivePreset(key);
      if (key === "year") {
        onChange(presets.year.getRange(selectedYear));
      } else {
        onChange(presets[key].getRange());
      }
    },
    [onChange, selectedYear],
  );

  const handleYearChange = useCallback(
    (yearStr: string) => {
      const year = Number(yearStr);
      setSelectedYear(year);
      setActivePreset("year");
      onChange(presets.year.getRange(year));
    },
    [onChange],
  );

  const displayLabel = useMemo(() => {
    if (activePreset === "year") {
      return String(selectedYear);
    }
    if (activePreset in presets) {
      return presets[activePreset].label;
    }
    if (value.startDate && value.endDate) {
      return `${value.startDate} - ${value.endDate}`;
    }
    return "Select range";
  }, [activePreset, selectedYear, value.startDate, value.endDate]);

  const handleSelectChange = useCallback(
    (val: string) => {
      handlePreset(val as PresetKey);
    },
    [handlePreset],
  );

  const nonYearPresets = (
    Object.keys(presets) as PresetKey[]
  ).filter((k) => k !== "year");

  return (
    <div className="flex items-center gap-2">
      {/* Dropdown for narrow screens */}
      <div className="sm:hidden">
        <Select value={activePreset} onValueChange={handleSelectChange}>
          <SelectTrigger size="sm">
            <SelectValue>{displayLabel}</SelectValue>
          </SelectTrigger>
          <SelectContent>
            {nonYearPresets.map((key) => (
              <SelectItem key={key} value={key}>
                {presets[key].label}
              </SelectItem>
            ))}
            <SelectItem value="year">Year</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Button row for wider screens */}
      <div className="hidden sm:flex items-center gap-2">
        {nonYearPresets.map((key) => (
          <Button
            key={key}
            variant={activePreset === key ? "default" : "outline"}
            size="sm"
            onClick={() => handlePreset(key)}
          >
            {presets[key].label}
          </Button>
        ))}
      </div>

      {/* Year dropdown */}
      <Select
        value={activePreset === "year" ? String(selectedYear) : ""}
        onValueChange={handleYearChange}
      >
        <SelectTrigger
          size="sm"
          className="w-[90px]"
          data-testid="year-dropdown"
        >
          <SelectValue placeholder="Year" />
        </SelectTrigger>
        <SelectContent>
          {availableYears.map((year) => (
            <SelectItem key={year} value={String(year)}>
              {year}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
