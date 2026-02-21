import { createContext } from "react";
import type { JwtPayload } from "@/lib/auth";

export interface AuthContextValue {
  user: JwtPayload | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    email: string,
    password: string,
    firstName?: string,
    lastName?: string,
  ) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
