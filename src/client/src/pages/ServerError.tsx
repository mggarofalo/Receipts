import { usePageTitle } from "@/hooks/usePageTitle";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
} from "@/components/ui/card";

function ServerError() {
  usePageTitle("Server Error");

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <p className="text-6xl font-bold" aria-hidden="true">500</p>
          <h1 className="text-lg font-semibold">Server Error</h1>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <p className="text-muted-foreground">
            Something went wrong on our end. Please try again later.
          </p>
          <Button onClick={() => window.location.assign("/")}>Reload</Button>
        </CardContent>
      </Card>
    </main>
  );
}

export default ServerError;
