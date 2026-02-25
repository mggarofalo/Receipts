import { usePageTitle } from "@/hooks/usePageTitle";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from "@/components/ui/card";

function Home() {
  usePageTitle("Home");

  return (
    <div className="flex items-center justify-center py-12">
      <Card className="w-[400px]">
        <CardHeader>
          <h1 className="text-lg font-semibold leading-none tracking-tight">Receipts</h1>
          <CardDescription>Receipt management application</CardDescription>
        </CardHeader>
        <CardContent>
          <Button>Get Started</Button>
        </CardContent>
      </Card>
    </div>
  );
}

export default Home;
