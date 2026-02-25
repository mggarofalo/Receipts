import { useTheme } from "next-themes";
import { Sun, Moon } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

function ThemeIcon() {
  const { theme, resolvedTheme } = useTheme();

  if (theme === "system") {
    const isDark = resolvedTheme === "dark";
    return (
      <>
        <Sun
          className={`h-4 w-4 transition-opacity ${isDark ? "opacity-40" : "opacity-100"}`}
        />
        <Moon
          className={`absolute h-4 w-4 transition-opacity ${isDark ? "opacity-100" : "opacity-40"}`}
        />
      </>
    );
  }

  if (theme === "dark") {
    return <Moon className="h-4 w-4" />;
  }

  return <Sun className="h-4 w-4" />;
}

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-8 w-8">
          <ThemeIcon />
          <span className="sr-only">Toggle theme</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem
          onClick={() => setTheme("light")}
          className={theme === "light" ? "font-semibold" : ""}
        >
          <Sun className="mr-2 h-4 w-4" />
          Light
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme("dark")}
          className={theme === "dark" ? "font-semibold" : ""}
        >
          <Moon className="mr-2 h-4 w-4" />
          Dark
        </DropdownMenuItem>
        <DropdownMenuItem
          onClick={() => setTheme("system")}
          className={theme === "system" ? "font-semibold" : ""}
        >
          <Sun className="mr-2 h-4 w-4" />
          <Moon className="-ml-4 h-4 w-4 opacity-60" />
          System
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
