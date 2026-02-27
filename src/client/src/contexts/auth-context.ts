import { createContext } from "react";
import type { JwtPayload } from "@/lib/auth";

export interface AuthContextValue {
  user: JwtPayload | null;
  isLoading: boolean;
  mustResetPassword: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  changePassword: (
    currentPassword: string,
    newPassword: string,
  ) => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
