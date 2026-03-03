import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import ChangePassword from "./ChangePassword";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

const mockChangePassword = vi.fn();

vi.mock("@/hooks/useAuth", () => ({
  useAuth: vi.fn(() => ({
    user: { email: "test@example.com" },
    mustResetPassword: true,
    changePassword: mockChangePassword,
  })),
}));

describe("ChangePassword", () => {
  it("renders the change password heading when user must reset", () => {
    renderWithProviders(<ChangePassword />);
    expect(
      screen.getByRole("heading", { name: /change password/i }),
    ).toBeInTheDocument();
  });

  it("renders current password, new password, and confirm password fields", () => {
    renderWithProviders(<ChangePassword />);
    expect(screen.getByLabelText(/^current password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^new password$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^confirm new password$/i)).toBeInTheDocument();
  });

  it("renders the change password submit button", () => {
    renderWithProviders(<ChangePassword />);
    expect(
      screen.getByRole("button", { name: /change password/i }),
    ).toBeInTheDocument();
  });

  it("shows description text about password requirement", () => {
    renderWithProviders(<ChangePassword />);
    expect(
      screen.getByText(/you must change your password before continuing/i),
    ).toBeInTheDocument();
  });
});
