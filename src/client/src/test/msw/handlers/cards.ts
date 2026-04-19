import { http, HttpResponse } from "msw";
import { cards } from "../fixtures";

export const cardHandlers = [
  http.get("*/api/cards", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const isActiveParam = url.searchParams.get("isActive");
    let filtered = cards;
    if (isActiveParam !== null) {
      const isActive = isActiveParam === "true";
      filtered = cards.filter((a) => a.isActive === isActive);
    }
    const page = filtered.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: filtered.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/cards/:id", ({ params }) => {
    const card = cards.find((a) => a.id === params.id);
    if (!card) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(card);
  }),

  http.post("*/api/cards", async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000001",
      ...body,
    });
  }),

  http.put("*/api/cards/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/cards/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
