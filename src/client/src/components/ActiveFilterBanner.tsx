import { Button } from "@/components/ui/button";

interface ActiveFilterBannerProps {
  message: string;
  onClear: () => void;
}

export function ActiveFilterBanner({ message, onClear }: ActiveFilterBannerProps) {
  return (
    <div className="flex items-center justify-between rounded-md border border-primary/20 bg-primary/5 px-4 py-2 text-sm">
      <span>{message}</span>
      <Button variant="ghost" size="sm" onClick={onClear}>
        Clear filter
      </Button>
    </div>
  );
}
