import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { CardSkeleton } from "./card-skeleton";

describe("CardSkeleton", () => {
  it("has role='status' for screen-reader announcement", () => {
    renderWithProviders(<CardSkeleton />);
    expect(screen.getByRole("status")).toBeInTheDocument();
  });

  it("has aria-live='polite' on the status container", () => {
    renderWithProviders(<CardSkeleton />);
    expect(screen.getByRole("status")).toHaveAttribute("aria-live", "polite");
  });

  it("has aria-busy='true' on the status container", () => {
    renderWithProviders(<CardSkeleton />);
    expect(screen.getByRole("status")).toHaveAttribute("aria-busy", "true");
  });

  it("renders an sr-only 'Loading…' label for assistive technology", () => {
    renderWithProviders(<CardSkeleton />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
    expect(screen.getByText("Loading…")).toHaveClass("sr-only");
  });

  it("marks the decorative Card as aria-hidden", () => {
    renderWithProviders(<CardSkeleton />);
    // The Card component renders a div; the immediate child of status div
    const statusEl = screen.getByRole("status");
    const card = statusEl.querySelector("[aria-hidden='true']");
    expect(card).toBeInTheDocument();
  });

  it("renders default 3 skeleton lines", () => {
    const { container } = renderWithProviders(<CardSkeleton />);
    const content = container.querySelector(".space-y-3");
    expect(content?.children).toHaveLength(3);
  });

  it("renders custom number of lines", () => {
    const { container } = renderWithProviders(<CardSkeleton lines={5} />);
    const content = container.querySelector(".space-y-3");
    expect(content?.children).toHaveLength(5);
  });

  describe("silent mode", () => {
    it("does not render role='status' when silent=true", () => {
      renderWithProviders(<CardSkeleton silent />);
      expect(screen.queryByRole("status")).not.toBeInTheDocument();
    });

    it("does not render the sr-only loading text when silent=true", () => {
      renderWithProviders(<CardSkeleton silent />);
      expect(screen.queryByText("Loading…")).not.toBeInTheDocument();
    });

    it("still renders skeleton lines when silent=true", () => {
      const { container } = renderWithProviders(<CardSkeleton lines={2} silent />);
      const content = container.querySelector(".space-y-3");
      expect(content?.children).toHaveLength(2);
    });

    it("does not mark card aria-hidden when silent=true", () => {
      const { container } = renderWithProviders(<CardSkeleton silent />);
      const card = container.querySelector("[aria-hidden='true']");
      expect(card).not.toBeInTheDocument();
    });
  });
});
