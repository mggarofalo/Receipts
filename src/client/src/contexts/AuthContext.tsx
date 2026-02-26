import { useCallback, useEffect, useMemo, useState } from "react";
import type { ReactNode } from "react";
import client from "@/lib/api-client";
import {
  clearTokens,
  getAccessToken,
  isAuthenticated as checkAuth,
  parseJwtPayload,
  setTokens,
  addTokenRefreshListener,
} from "@/lib/auth";
import type { JwtPayload } from "@/lib/auth";
import { AuthContext } from "@/contexts/auth-context";

function getInitialUser(): JwtPayload | null {
  if (!checkAuth()) return null;
  const token = getAccessToken();
  return token ? parseJwtPayload(token) : null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<JwtPayload | null>(getInitialUser);
  const [mustResetPassword, setMustResetPassword] = useState(false);

  useEffect(() => {
    return addTokenRefreshListener(() => {
      const token = getAccessToken();
      setUser(token ? parseJwtPayload(token) : null);
    });
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const { data, error } = await client.POST("/api/auth/login", {
      body: { email, password },
    });
    if (error) {
      throw error;
    }
    if (data) {
      setTokens(data.accessToken, data.refreshToken);
      setUser(parseJwtPayload(data.accessToken));
      setMustResetPassword(data.mustResetPassword);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await client.POST("/api/auth/logout");
    } catch {
      // Best effort — clear tokens regardless
    }
    clearTokens();
    setUser(null);
    setMustResetPassword(false);
  }, []);

  const changePassword = useCallback(
    async (currentPassword: string, newPassword: string) => {
      const { data, error } = await client.POST(
        "/api/auth/change-password",
        {
          body: { currentPassword, newPassword },
        },
      );
      if (error) {
        throw error;
      }
      if (data) {
        setTokens(data.accessToken, data.refreshToken);
        setUser(parseJwtPayload(data.accessToken));
        setMustResetPassword(false);
      }
    },
    [],
  );

  const value = useMemo(
    () => ({
      user,
      isLoading: false,
      mustResetPassword,
      login,
      logout,
      changePassword,
    }),
    [user, mustResetPassword, login, logout, changePassword],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
