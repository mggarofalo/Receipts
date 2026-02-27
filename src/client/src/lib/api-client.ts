import createClient from "openapi-fetch";
import type { Middleware } from "openapi-fetch";
import type { paths } from "@/generated/api";
import {
  getAccessToken,
  getRefreshToken,
  setTokens,
  clearTokens,
  notifyTokenRefresh,
} from "@/lib/auth";

const baseUrl = import.meta.env.VITE_API_URL ?? "";

const client = createClient<paths>({ baseUrl });

let refreshPromise: Promise<boolean> | null = null;

async function attemptTokenRefresh(): Promise<boolean> {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  try {
    const res = await fetch(`${baseUrl}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });
    if (!res.ok) return false;

    const data = await res.json();
    setTokens(data.accessToken, data.refreshToken);
    notifyTokenRefresh();
    return true;
  } catch {
    return false;
  }
}

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = getAccessToken();
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`);
    }
    return request;
  },
  async onResponse({ request, response }) {
    if (response.status !== 401) return response;

    // Avoid refresh loop for auth endpoints
    const url = new URL(request.url);
    if (url.pathname.startsWith("/api/auth/")) return response;

    // Deduplicate concurrent refresh attempts
    if (!refreshPromise) {
      refreshPromise = attemptTokenRefresh().finally(() => {
        refreshPromise = null;
      });
    }

    const refreshed = await refreshPromise;
    if (!refreshed) {
      clearTokens();
      window.location.href = "/login";
      return response;
    }

    // Retry original request with new token
    const newToken = getAccessToken();
    const retryRequest = new Request(request, {
      headers: new Headers(request.headers),
    });
    if (newToken) {
      retryRequest.headers.set("Authorization", `Bearer ${newToken}`);
    }
    return fetch(retryRequest);
  },
};

client.use(authMiddleware);

export default client;
