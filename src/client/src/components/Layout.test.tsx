import React from "react";
import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";

// vi.hoisted() runs before vi.mock factories — variables referenced in mock
// factories must be created here to avoid temporal dead zone errors.
const { mockUseSignalR } = vi.hoisted(() => ({
  mockUseSignalR: vi.fn(),
}));

// Mock react-router so Layout renders without a RouterProvider
vi.mock("react-router", () => ({
  Link: ({ children }: { children: React.ReactNode }) => (
    <span>{children}</span>
  ),
  Outlet: () => null,
  useNavigate: () => vi.fn(),
}));

vi.mock("@/hooks/useAuth", () => ({
  useAuth: vi.fn(() => ({ user: null, logout: vi.fn() })),
}));

vi.mock("@/hooks/usePermission", () => ({
  usePermission: vi.fn(() => ({ isAdmin: () => false })),
}));

vi.mock("@/hooks/useSignalR", () => ({
  useSignalR: mockUseSignalR,
}));

// Mock shadcn/Radix UI components that need browser APIs unavailable in jsdom
vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    ...props
  }: React.ButtonHTMLAttributes<HTMLButtonElement>) => (
    <button {...props}>{children}</button>
  ),
}));

vi.mock("@/components/ui/dropdown-menu", () => ({
  DropdownMenu: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuContent: ({ children }: { children: React.ReactNode }) => (
    <div>{children}</div>
  ),
  DropdownMenuItem: ({
    children,
    onClick,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
  }) => <div onClick={onClick}>{children}</div>,
  DropdownMenuSeparator: () => <hr />,
  DropdownMenuTrigger: ({
    children,
  }: {
    children: React.ReactNode;
    asChild?: boolean;
  }) => <div>{children}</div>,
}));

vi.mock("@/components/ui/separator", () => ({
  Separator: () => <hr />,
}));

import { Layout } from "./Layout";

describe("Layout – connection status indicator is visible to the user", () => {
  afterEach(cleanup);

  it('shows "Live" label when SignalR is connected', () => {
    mockUseSignalR.mockReturnValue({ connectionState: "connected" });
    render(<Layout />);
    expect(screen.getByText("Live")).toBeInTheDocument();
  });

  it('shows "Reconnecting" label when SignalR is reconnecting', () => {
    mockUseSignalR.mockReturnValue({ connectionState: "reconnecting" });
    render(<Layout />);
    expect(screen.getByText("Reconnecting")).toBeInTheDocument();
  });

  it('shows "Offline" label when SignalR is disconnected', () => {
    mockUseSignalR.mockReturnValue({ connectionState: "disconnected" });
    render(<Layout />);
    expect(screen.getByText("Offline")).toBeInTheDocument();
  });
});
