import { describe, it, expect } from "vitest";
import { screen } from "@testing-library/react";
import { ProtectedRoute } from "./ProtectedRoute";
import { renderWithProviders } from "@/test/test-utils";

describe("ProtectedRoute", () => {
  it("renders children when user is authenticated", () => {
    renderWithProviders(<ProtectedRoute>Dashboard</ProtectedRoute>, {
      auth: { user: { userId: "user-id", email: "user@test.com", roles: ["User"], mustResetPassword: false } },
    });
    expect(screen.getByText("Dashboard")).toBeInTheDocument();
  });

  it("shows loading state when auth is loading", () => {
    renderWithProviders(<ProtectedRoute>Dashboard</ProtectedRoute>, {
      auth: { isLoading: true },
    });
    expect(screen.getByText("Loading...")).toBeInTheDocument();
    expect(screen.queryByText("Dashboard")).not.toBeInTheDocument();
  });

  it("redirects to /login when user is null", () => {
    renderWithProviders(<ProtectedRoute>Dashboard</ProtectedRoute>, {
      auth: { user: null },
    });
    expect(screen.queryByText("Dashboard")).not.toBeInTheDocument();
  });

  it("redirects to /change-password when mustResetPassword is true", () => {
    renderWithProviders(<ProtectedRoute>Dashboard</ProtectedRoute>, {
      auth: {
        user: { userId: "user-id", email: "user@test.com", roles: ["User"], mustResetPassword: false },
        mustResetPassword: true,
      },
    });
    expect(screen.queryByText("Dashboard")).not.toBeInTheDocument();
  });
});
