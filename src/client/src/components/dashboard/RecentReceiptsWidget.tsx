import { ChartCard } from "@/components/charts";

interface RecentReceiptsWidgetProps {
  className?: string;
}

export function RecentReceiptsWidget({ className }: RecentReceiptsWidgetProps) {
  return (
    <ChartCard title="Recent Receipts" className={className}>
      <p className="text-muted-foreground text-sm">Recent receipts will appear here.</p>
    </ChartCard>
  );
}
