import { useNavigate } from "react-router";
import { usePageTitle } from "@/hooks/usePageTitle";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
} from "@/components/ui/card";

function NotFound() {
  usePageTitle("Page Not Found");
  const navigate = useNavigate();

  return (
    <main className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <p className="text-6xl font-bold" aria-hidden="true">404</p>
          <h1 className="text-lg font-semibold">Page Not Found</h1>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <p className="text-muted-foreground">
            The page you are looking for does not exist or has been moved.
          </p>
          <Button onClick={() => navigate("/")}>Go Home</Button>
        </CardContent>
      </Card>
    </main>
  );
}

export default NotFound;
