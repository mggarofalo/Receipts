import { useState, useCallback } from "react";
import { format, subDays } from "date-fns";
import { usePageTitle } from "@/hooks/usePageTitle";
import type { DateRange } from "@/hooks/useDashboard";
import { DateRangeSelector } from "@/components/dashboard/DateRangeSelector";
import { SummaryStats } from "@/components/dashboard/SummaryStats";
import { SpendingOverTimeWidget } from "@/components/dashboard/SpendingOverTimeWidget";
import { SpendingByCategoryWidget } from "@/components/dashboard/SpendingByCategoryWidget";
import { SpendingByAccountWidget } from "@/components/dashboard/SpendingByAccountWidget";
import { RecentReceiptsWidget } from "@/components/dashboard/RecentReceiptsWidget";

function getDefaultRange(): DateRange {
  const now = new Date();
  return {
    startDate: format(subDays(now, 30), "yyyy-MM-dd"),
    endDate: format(now, "yyyy-MM-dd"),
  };
}

function Dashboard() {
  usePageTitle("Dashboard");
  const [dateRange, setDateRange] = useState<DateRange>(getDefaultRange);

  const handleDateRangeChange = useCallback((range: DateRange) => {
    setDateRange(range);
  }, []);

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
        <DateRangeSelector value={dateRange} onChange={handleDateRangeChange} />
      </div>

      <SummaryStats dateRange={dateRange} />

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <SpendingOverTimeWidget
          dateRange={dateRange}
          className="md:col-span-2"
        />
        <SpendingByCategoryWidget dateRange={dateRange} />
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <SpendingByAccountWidget dateRange={dateRange} />
        <RecentReceiptsWidget />
      </div>
    </div>
  );
}

export default Dashboard;
