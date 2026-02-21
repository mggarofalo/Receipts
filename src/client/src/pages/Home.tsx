import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

function Home() {
  return (
    <div className="flex items-center justify-center py-12">
      <Card className="w-[400px]">
        <CardHeader>
          <CardTitle>Receipts</CardTitle>
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
