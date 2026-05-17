import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { TableSkeleton } from "./table-skeleton";

describe("TableSkeleton", () => {
  it("has role='status' for screen-reader announcement", () => {
    renderWithProviders(<TableSkeleton />);
    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("has aria-live='polite' on the status container", () => {
    renderWithProviders(<TableSkeleton />);
    expect(screen.getByRole("status")).toHaveAttribute("aria-live", "polite");
  });

  it("has aria-busy='true' on the status container", () => {
    renderWithProviders(<TableSkeleton />);
    expect(screen.getByRole("status")).toHaveAttribute("aria-busy", "true");
  });

  it("renders an sr-only 'Loading…' label for assistive technology", () => {
    renderWithProviders(<TableSkeleton />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
    expect(screen.getByText("Loading…")).toHaveClass("sr-only");
  });

  it("renders the toolbar by default", () => {
    const { container } = renderWithProviders(<TableSkeleton />);
    // Toolbar is marked aria-hidden so use container query
    const toolbar = container.querySelector(".flex.items-center.justify-between");
    expect(toolbar).toBeInTheDocument();
    expect(toolbar).toHaveAttribute("aria-hidden", "true");
  });

  it("omits the toolbar when showToolbar=false", () => {
    const { container } = renderWithProviders(<TableSkeleton showToolbar={false} />);
    const toolbar = container.querySelector(".flex.items-center.justify-between");
    expect(toolbar).not.toBeInTheDocument();
  });

  it("renders the correct number of rows", () => {
    const { container } = renderWithProviders(<TableSkeleton rows={3} columns={2} />);
    // Each row is a div inside .divide-y
    const rowContainer = container.querySelector(".divide-y");
    expect(rowContainer?.children).toHaveLength(3);
  });

  it("decorative table container is aria-hidden", () => {
    const { container } = renderWithProviders(<TableSkeleton />);
    const tableContainer = container.querySelector(".rounded-md.border");
    expect(tableContainer).toHaveAttribute("aria-hidden", "true");
  });
});
