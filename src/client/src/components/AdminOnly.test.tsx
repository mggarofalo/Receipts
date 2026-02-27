import { describe, it, expect } from "vitest";
import { screen } from "@testing-library/react";
import { AdminOnly } from "./AdminOnly";
import { renderWithProviders } from "@/test/test-utils";

describe("AdminOnly", () => {
  it("renders children when user is admin", () => {
    renderWithProviders(<AdminOnly>Secret content</AdminOnly>, {
      auth: { user: { email: "admin@test.com", roles: ["Admin"] } },
    });
    expect(screen.getByText("Secret content")).toBeInTheDocument();
  });

  it("renders nothing when user is not admin", () => {
    renderWithProviders(<AdminOnly>Secret content</AdminOnly>, {
      auth: { user: { email: "user@test.com", roles: ["User"] } },
    });
    expect(screen.queryByText("Secret content")).not.toBeInTheDocument();
  });

  it("renders nothing when user is null", () => {
    renderWithProviders(<AdminOnly>Secret content</AdminOnly>, {
      auth: { user: null },
    });
    expect(screen.queryByText("Secret content")).not.toBeInTheDocument();
  });
});
