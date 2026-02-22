import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

function ServerError() {
  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <CardTitle className="text-6xl font-bold">500</CardTitle>
          <CardDescription className="text-lg">Server error</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <p className="text-muted-foreground">
            Something went wrong on our end. Please try again later.
          </p>
          <Button onClick={() => window.location.assign("/")}>Reload</Button>
        </CardContent>
      </Card>
    </div>
  );
}

export default ServerError;
