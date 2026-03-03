import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import NotFound from "./NotFound";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

describe("NotFound", () => {
  it("renders the 404 text", () => {
    renderWithProviders(<NotFound />);
    expect(screen.getByText("404")).toBeInTheDocument();
  });

  it("renders the page not found heading", () => {
    renderWithProviders(<NotFound />);
    expect(
      screen.getByRole("heading", { name: /page not found/i }),
    ).toBeInTheDocument();
  });

  it("renders the description text", () => {
    renderWithProviders(<NotFound />);
    expect(
      screen.getByText(/the page you are looking for does not exist/i),
    ).toBeInTheDocument();
  });

  it("renders the Go Home button", () => {
    renderWithProviders(<NotFound />);
    expect(
      screen.getByRole("button", { name: /go home/i }),
    ).toBeInTheDocument();
  });
});
