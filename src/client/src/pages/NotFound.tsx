import { useNavigate } from "react-router";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

function NotFound() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen items-center justify-center p-4">
      <Card className="w-full max-w-md text-center">
        <CardHeader>
          <CardTitle className="text-6xl font-bold">404</CardTitle>
          <CardDescription className="text-lg">Page not found</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <p className="text-muted-foreground">
            The page you are looking for does not exist or has been moved.
          </p>
          <Button onClick={() => navigate("/")}>Go Home</Button>
        </CardContent>
      </Card>
    </div>
  );
}

export default NotFound;
