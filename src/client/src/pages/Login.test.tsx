import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import Login from "./Login";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAuth", () => ({
  useAuth: vi.fn(() => ({
    user: null,
    mustResetPassword: false,
    login: vi.fn(),
  })),
}));

describe("Login", () => {
  it("renders the sign in heading", () => {
    renderWithProviders(<Login />);
    expect(
      screen.getByRole("heading", { name: /sign in/i }),
    ).toBeInTheDocument();
  });

  it("renders email and password fields", () => {
    renderWithProviders(<Login />);
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });

  it("renders the sign in button", () => {
    renderWithProviders(<Login />);
    expect(
      screen.getByRole("button", { name: /sign in/i }),
    ).toBeInTheDocument();
  });

  it("renders the description text", () => {
    renderWithProviders(<Login />);
    expect(
      screen.getByText(/enter your credentials/i),
    ).toBeInTheDocument();
  });
});
