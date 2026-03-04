import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import NotFound from "./NotFound";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof import("react-router")>();
  return {
    ...actual,
    useNavigate: vi.fn(() => vi.fn()),
  };
});

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

  it("calls navigate when Go Home button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockNavigate = vi.fn();
    const { useNavigate } = await import("react-router");
    vi.mocked(useNavigate).mockReturnValue(mockNavigate);

    renderWithProviders(<NotFound />);
    await user.click(screen.getByRole("button", { name: /go home/i }));

    expect(mockNavigate).toHaveBeenCalledWith("/");
  });
});
