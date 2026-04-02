import { http, HttpResponse } from "msw";
import { categories } from "../fixtures";

export const categoryHandlers = [
  http.get("*/api/categories", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const isActiveParam = url.searchParams.get("isActive");
    let filtered = categories;
    if (isActiveParam !== null) {
      const isActive = isActiveParam === "true";
      filtered = categories.filter((c) => c.isActive === isActive);
    }
    const page = filtered.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: filtered.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/categories/:id", ({ params }) => {
    const category = categories.find((c) => c.id === params.id);
    if (!category) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(category);
  }),

  http.post("*/api/categories", async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000002",
      ...body,
    });
  }),

  http.put("*/api/categories/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/categories/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
