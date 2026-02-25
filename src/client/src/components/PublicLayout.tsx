import { Link, Outlet } from "react-router";
import { ThemeToggle } from "@/components/ThemeToggle";

export function PublicLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b">
        <div className="container mx-auto flex h-14 items-center justify-between px-4">
          <Link to="/" className="font-semibold text-lg">
            Receipts
          </Link>
          <ThemeToggle />
        </div>
      </header>
      <main className="flex-1 flex items-center justify-center p-4">
        <Outlet />
      </main>
    </div>
  );
}
