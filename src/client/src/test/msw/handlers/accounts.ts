import { http, HttpResponse } from "msw";
import { accounts } from "../fixtures";

export const accountHandlers = [
  http.get("*/api/accounts", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const page = accounts.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: accounts.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/accounts/:id", ({ params }) => {
    const account = accounts.find((a) => a.id === params.id);
    if (!account) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(account);
  }),

  http.post("*/api/accounts", async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000001",
      ...body,
    });
  }),

  http.put("*/api/accounts/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/accounts/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
