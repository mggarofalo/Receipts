import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import Home from "./Home";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

describe("Home", () => {
  it("renders the page heading", () => {
    renderWithProviders(<Home />);
    expect(
      screen.getByRole("heading", { name: /receipts/i }),
    ).toBeInTheDocument();
  });

  it("renders the description text", () => {
    renderWithProviders(<Home />);
    expect(
      screen.getByText(/receipt management application/i),
    ).toBeInTheDocument();
  });

  it("renders the Get Started button", () => {
    renderWithProviders(<Home />);
    expect(
      screen.getByRole("button", { name: /get started/i }),
    ).toBeInTheDocument();
  });

  it("calls usePageTitle with Home", async () => {
    const { usePageTitle } = await import("@/hooks/usePageTitle");
    renderWithProviders(<Home />);
    expect(usePageTitle).toHaveBeenCalledWith("Home");
  });
});
