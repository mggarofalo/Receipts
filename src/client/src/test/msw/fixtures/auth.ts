import type { components } from "@/generated/api";

type TokenResponse = components["schemas"]["TokenResponse"];
type LoginRequest = components["schemas"]["LoginRequest"];

export const loginRequest: LoginRequest = {
  email: "test@example.com",
  password: "Password123!",
};

export const tokenResponse: TokenResponse = {
  accessToken: "fake-access-token-for-testing",
  refreshToken: "fake-refresh-token-for-testing",
  expiresIn: 3600,
  mustResetPassword: false,
  tokenType: "Bearer",
  scope: "",
};
