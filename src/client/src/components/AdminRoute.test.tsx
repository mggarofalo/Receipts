import { describe, it, expect } from "vitest";
import { screen } from "@testing-library/react";
import { AdminRoute } from "./AdminRoute";
import { renderWithProviders } from "@/test/test-utils";

describe("AdminRoute", () => {
  it("renders children when user is admin", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { userId: "admin-id", email: "admin@test.com", roles: ["Admin"], mustResetPassword: false } },
    });
    expect(screen.getByText("Admin page")).toBeInTheDocument();
  });

  it("redirects to / when user is not admin", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { userId: "user-id", email: "user@test.com", roles: ["User"], mustResetPassword: false } },
    });
    expect(screen.queryByText("Admin page")).not.toBeInTheDocument();
  });

  it("redirects when user has no roles", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { userId: "user-id", email: "user@test.com", roles: [], mustResetPassword: false } },
    });
    expect(screen.queryByText("Admin page")).not.toBeInTheDocument();
  });
});
