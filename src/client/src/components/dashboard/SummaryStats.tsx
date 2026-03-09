import { Receipt, DollarSign, TrendingUp, Tag } from "lucide-react";
import { useDashboardSummary } from "@/hooks/useDashboard";
import type { DateRange } from "@/hooks/useDashboard";
import { StatCard } from "./StatCard";

interface SummaryStatsProps {
  dateRange: DateRange;
  className?: string;
}

function formatCurrency(value: number | string | undefined): string {
  const num = Number(value ?? 0);
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(num);
}

export function SummaryStats({ dateRange, className }: SummaryStatsProps) {
  const { data, isLoading } = useDashboardSummary(dateRange);

  return (
    <div
      className={`grid gap-4 sm:grid-cols-2 lg:grid-cols-4 ${className ?? ""}`}
    >
      <StatCard
        title="Total Receipts"
        value={String(Number(data?.totalReceipts ?? 0))}
        icon={Receipt}
        loading={isLoading}
      />
      <StatCard
        title="Total Spent"
        value={formatCurrency(data?.totalSpent)}
        icon={DollarSign}
        loading={isLoading}
      />
      <StatCard
        title="Avg Trip Amount"
        value={formatCurrency(data?.averageTripAmount)}
        icon={TrendingUp}
        loading={isLoading}
      />
      <StatCard
        title="Top Category"
        value={data?.mostUsedCategory?.name ?? "—"}
        subtitle={
          data?.mostUsedCategory?.count
            ? `${Number(data.mostUsedCategory.count)} receipts`
            : undefined
        }
        icon={Tag}
        loading={isLoading}
      />
    </div>
  );
}
