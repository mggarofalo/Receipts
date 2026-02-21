import { useCallback, useMemo, useState } from "react";
import type { ReactNode } from "react";
import client from "@/lib/api-client";
import {
  clearTokens,
  getAccessToken,
  isAuthenticated as checkAuth,
  parseJwtPayload,
  setTokens,
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
    }
  }, []);

  const register = useCallback(
    async (
      email: string,
      password: string,
      firstName?: string,
      lastName?: string,
    ) => {
      const { data, error } = await client.POST("/api/auth/register", {
        body: { email, password, firstName, lastName },
      });
      if (error) {
        throw error;
      }
      if (data) {
        setTokens(data.accessToken, data.refreshToken);
        setUser(parseJwtPayload(data.accessToken));
      }
    },
    [],
  );

  const logout = useCallback(async () => {
    try {
      await client.POST("/api/auth/logout");
    } catch {
      // Best effort — clear tokens regardless
    }
    clearTokens();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({ user, isLoading: false, login, register, logout }),
    [user, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
