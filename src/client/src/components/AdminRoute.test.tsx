import { describe, it, expect } from "vitest";
import { screen } from "@testing-library/react";
import { AdminRoute } from "./AdminRoute";
import { renderWithProviders } from "@/test/test-utils";

describe("AdminRoute", () => {
  it("renders children when user is admin", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { email: "admin@test.com", roles: ["Admin"] } },
    });
    expect(screen.getByText("Admin page")).toBeInTheDocument();
  });

  it("redirects to / when user is not admin", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { email: "user@test.com", roles: ["User"] } },
    });
    expect(screen.queryByText("Admin page")).not.toBeInTheDocument();
  });

  it("redirects when user has no roles", () => {
    renderWithProviders(<AdminRoute>Admin page</AdminRoute>, {
      auth: { user: { email: "user@test.com", roles: [] } },
    });
    expect(screen.queryByText("Admin page")).not.toBeInTheDocument();
  });
});
