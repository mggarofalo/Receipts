import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { Breadcrumbs } from "./Breadcrumbs";

vi.mock("@/hooks/useBreadcrumbs", () => ({
  useBreadcrumbs: vi.fn(),
}));

import { useBreadcrumbs } from "@/hooks/useBreadcrumbs";

describe("Breadcrumbs", () => {
  it("renders nothing when crumbs array is empty", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([]);
    const { container } = renderWithProviders(<Breadcrumbs />);
    expect(container.firstChild).toBeNull();
  });

  it("renders a nav element with breadcrumb aria-label", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([
      { label: "Home", path: "/" },
      { label: "Accounts", path: "/accounts" },
    ]);
    renderWithProviders(<Breadcrumbs />);
    expect(screen.getByLabelText("Breadcrumb")).toBeInTheDocument();
  });

  it("renders the last crumb as text with aria-current='page'", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([
      { label: "Home", path: "/" },
      { label: "Accounts", path: "/accounts" },
    ]);
    renderWithProviders(<Breadcrumbs />);

    const currentPage = screen.getByText("Accounts");
    expect(currentPage).toHaveAttribute("aria-current", "page");
  });

  it("renders non-last crumbs as links", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([
      { label: "Home", path: "/" },
      { label: "Receipts", path: "/receipts" },
    ]);
    renderWithProviders(<Breadcrumbs />);

    const homeLink = screen.getByText("Home");
    expect(homeLink.closest("a")).toHaveAttribute("href", "/");
  });

  it("renders separator between crumbs", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([
      { label: "Home", path: "/" },
      { label: "Accounts", path: "/accounts" },
    ]);
    renderWithProviders(<Breadcrumbs />);

    const separator = screen.getByText("/");
    expect(separator).toHaveAttribute("aria-hidden", "true");
  });

  it("renders multiple breadcrumb segments", () => {
    vi.mocked(useBreadcrumbs).mockReturnValue([
      { label: "Home", path: "/" },
      { label: "Admin", path: "/admin" },
      { label: "Users", path: "/admin/users" },
    ]);
    renderWithProviders(<Breadcrumbs />);

    expect(screen.getByText("Home")).toBeInTheDocument();
    expect(screen.getByText("Admin")).toBeInTheDocument();
    expect(screen.getByText("Users")).toBeInTheDocument();
  });
});
