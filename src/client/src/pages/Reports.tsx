import { lazy, Suspense, useMemo } from "react";
import { useSearchParams } from "react-router";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";

interface ReportConfig {
  slug: string;
  name: string;
  component: React.LazyExoticComponent<React.ComponentType>;
}

const REPORTS: ReportConfig[] = [
  {
    slug: "out-of-balance",
    name: "Out of Balance",
    component: lazy(() => import("@/components/reports/OutOfBalance")),
  },
  {
    slug: "item-similarity",
    name: "Item Similarity",
    component: lazy(() => import("@/components/reports/ItemSimilarity")),
  },
  {
    slug: "item-cost-over-time",
    name: "Item Cost Over Time",
    component: lazy(() => import("@/components/reports/ItemCostOverTime")),
  },
  {
    slug: "spending-by-location",
    name: "Spending by Location",
    component: lazy(() => import("@/components/reports/SpendingByLocation")),
  },
  {
    slug: "category-trends",
    name: "Category Trends",
    component: lazy(() => import("@/components/reports/CategoryTrends")),
  },
  {
    slug: "duplicate-detection",
    name: "Duplicate Detection",
    component: lazy(() => import("@/components/reports/DuplicateDetection")),
  },
  {
    slug: "uncategorized-items",
    name: "Uncategorized Items",
    component: lazy(() => import("@/components/reports/UncategorizedItems")),
  },
];

const DEFAULT_REPORT = REPORTS[0].slug;
const VALID_SLUGS = new Set(REPORTS.map((r) => r.slug));

function ReportFallback() {
  return <Skeleton className="h-32 w-full rounded-lg" />;
}

function Reports() {
  const [searchParams, setSearchParams] = useSearchParams();
  const rawReport = searchParams.get("report");
  const activeSlug =
    rawReport && VALID_SLUGS.has(rawReport) ? rawReport : DEFAULT_REPORT;

  const activeReport = useMemo(
    () => REPORTS.find((r) => r.slug === activeSlug)!,
    [activeSlug],
  );

  usePageTitle(`Reports - ${activeReport.name}`);

  function handleTabChange(slug: string) {
    setSearchParams({ report: slug }, { replace: true });
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold tracking-tight">Reports</h1>
      <Tabs value={activeSlug} onValueChange={handleTabChange}>
        <TabsList className="flex-wrap">
          {REPORTS.map((report) => (
            <TabsTrigger key={report.slug} value={report.slug}>
              {report.name}
            </TabsTrigger>
          ))}
        </TabsList>
        {REPORTS.map((report) => (
          <TabsContent key={report.slug} value={report.slug}>
            <Suspense fallback={<ReportFallback />}>
              <report.component />
            </Suspense>
          </TabsContent>
        ))}
      </Tabs>
    </div>
  );
}

export default Reports;
export { REPORTS, DEFAULT_REPORT };
