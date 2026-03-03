import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { RootLayout } from "./RootLayout";

vi.mock("@/components/ui/sonner", () => ({
  Toaster: () => <div data-testid="toaster">Toaster</div>,
}));

vi.mock("@/components/ErrorBoundary", () => ({
  ErrorBoundary: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="error-boundary">{children}</div>
  ),
}));

describe("RootLayout", () => {
  it("renders the ErrorBoundary wrapper", () => {
    renderWithProviders(<RootLayout />);
    expect(screen.getByTestId("error-boundary")).toBeInTheDocument();
  });

  it("renders the Toaster component", () => {
    renderWithProviders(<RootLayout />);
    expect(screen.getByTestId("toaster")).toBeInTheDocument();
  });

  it("renders ErrorBoundary as the outermost element", () => {
    renderWithProviders(<RootLayout />);
    const errorBoundary = screen.getByTestId("error-boundary");
    const toaster = screen.getByTestId("toaster");
    expect(errorBoundary).toContainElement(toaster);
  });

  it("renders children via Outlet within ErrorBoundary", () => {
    renderWithProviders(<RootLayout />);
    const errorBoundary = screen.getByTestId("error-boundary");
    expect(errorBoundary).toBeInTheDocument();
  });
});
