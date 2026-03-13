import { http, HttpResponse } from "msw";
import { tokenResponse } from "../fixtures";

export const authHandlers = [
  http.post("*/api/auth/login", () => {
    return HttpResponse.json(tokenResponse);
  }),

  http.post("*/api/auth/refresh", () => {
    return HttpResponse.json({
      ...tokenResponse,
      accessToken: "refreshed-access-token-for-testing",
      refreshToken: "refreshed-refresh-token-for-testing",
    });
  }),

  http.post("*/api/auth/logout", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
