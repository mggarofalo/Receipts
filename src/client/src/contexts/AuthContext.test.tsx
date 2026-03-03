import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, act } from "@testing-library/react";
import { useContext } from "react";
import { AuthProvider } from "./AuthContext";
import { AuthContext } from "./auth-context";

vi.mock("@/lib/api-client", () => {
  const mockClient = {
    GET: vi.fn(),
    POST: vi.fn(),
    PUT: vi.fn(),
    DELETE: vi.fn(),
    use: vi.fn(),
  };
  return { default: mockClient };
});

vi.mock("@/lib/auth", () => ({
  isAuthenticated: vi.fn(() => false),
  getAccessToken: vi.fn(() => null),
  parseJwtPayload: vi.fn(() => null),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  addTokenRefreshListener: vi.fn(() => vi.fn()),
}));

import client from "@/lib/api-client";
import * as auth from "@/lib/auth";

const mockedClient = vi.mocked(client);
const mockedAuth = vi.mocked(auth);

// Helper JWT: header.payload.signature with payload = { email, role }
function makeJwt(email: string, role: string): string {
  const payload = btoa(JSON.stringify({ email, role }));
  return `header.${payload}.signature`;
}

function TestConsumer() {
  const ctx = useContext(AuthContext);
  if (!ctx) return <div>no context</div>;
  return (
    <div>
      <span data-testid="user">{ctx.user?.email ?? "null"}</span>
      <span data-testid="loading">{String(ctx.isLoading)}</span>
      <span data-testid="must-reset">{String(ctx.mustResetPassword)}</span>
      <button data-testid="login" onClick={() => ctx.login("a@b.com", "pw")} />
      <button data-testid="logout" onClick={() => ctx.logout()} />
      <button
        data-testid="change-pw"
        onClick={() => ctx.changePassword("old", "new")}
      />
    </div>
  );
}

describe("AuthProvider", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedAuth.isAuthenticated.mockReturnValue(false);
    mockedAuth.getAccessToken.mockReturnValue(null);
    mockedAuth.parseJwtPayload.mockReturnValue(null);
    mockedAuth.addTokenRefreshListener.mockReturnValue(vi.fn());
  });

  it("provides initial null user when no token is stored", () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("user")).toHaveTextContent("null");
    expect(screen.getByTestId("loading")).toHaveTextContent("false");
    expect(screen.getByTestId("must-reset")).toHaveTextContent("false");
  });

  it("parses stored token on mount and provides user", () => {
    const token = makeJwt("user@test.com", "User");
    mockedAuth.isAuthenticated.mockReturnValue(true);
    mockedAuth.getAccessToken.mockReturnValue(token);
    mockedAuth.parseJwtPayload.mockReturnValue({
      email: "user@test.com",
      roles: ["User"],
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("user")).toHaveTextContent("user@test.com");
  });

  it("login calls POST /api/auth/login, stores tokens, and sets user", async () => {
    const token = makeJwt("a@b.com", "User");
    mockedClient.POST.mockResolvedValueOnce({
      data: {
        accessToken: token,
        refreshToken: "rt-123",
        mustResetPassword: false,
      },
      error: undefined,
    });
    mockedAuth.parseJwtPayload.mockReturnValue({
      email: "a@b.com",
      roles: ["User"],
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByTestId("login").click();
    });

    expect(mockedClient.POST).toHaveBeenCalledWith("/api/auth/login", {
      body: { email: "a@b.com", password: "pw" },
    });
    expect(mockedAuth.setTokens).toHaveBeenCalledWith(token, "rt-123");
    expect(screen.getByTestId("user")).toHaveTextContent("a@b.com");
  });

  it("logout clears tokens and sets user to null", async () => {
    const token = makeJwt("user@test.com", "User");
    mockedAuth.isAuthenticated.mockReturnValue(true);
    mockedAuth.getAccessToken.mockReturnValue(token);
    mockedAuth.parseJwtPayload.mockReturnValue({
      email: "user@test.com",
      roles: ["User"],
    });
    mockedClient.POST.mockResolvedValueOnce({
      data: undefined,
      error: undefined,
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("user")).toHaveTextContent("user@test.com");

    await act(async () => {
      screen.getByTestId("logout").click();
    });

    expect(mockedAuth.clearTokens).toHaveBeenCalled();
    expect(screen.getByTestId("user")).toHaveTextContent("null");
  });

  it("changePassword calls POST /api/auth/change-password and updates user", async () => {
    const newToken = makeJwt("a@b.com", "User");
    mockedClient.POST.mockResolvedValueOnce({
      data: {
        accessToken: newToken,
        refreshToken: "rt-new",
        mustResetPassword: false,
      },
      error: undefined,
    });
    mockedAuth.parseJwtPayload.mockReturnValue({
      email: "a@b.com",
      roles: ["User"],
    });

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByTestId("change-pw").click();
    });

    expect(mockedClient.POST).toHaveBeenCalledWith(
      "/api/auth/change-password",
      {
        body: { currentPassword: "old", newPassword: "new" },
      },
    );
    expect(mockedAuth.setTokens).toHaveBeenCalledWith(newToken, "rt-new");
    expect(screen.getByTestId("user")).toHaveTextContent("a@b.com");
  });
});
