import { http, HttpResponse } from "msw";
import { subcategories } from "../fixtures";

export const subcategoryHandlers = [
  http.get("*/api/subcategories", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const isActiveParam = url.searchParams.get("isActive");
    const categoryId = url.searchParams.get("categoryId");
    let filtered = subcategories;
    if (isActiveParam !== null) {
      const isActive = isActiveParam === "true";
      filtered = filtered.filter((s) => s.isActive === isActive);
    }
    if (categoryId !== null) {
      filtered = filtered.filter((s) => s.categoryId === categoryId);
    }
    const page = filtered.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: filtered.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/subcategories/:id", ({ params }) => {
    const subcategory = subcategories.find((s) => s.id === params.id);
    if (!subcategory) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(subcategory);
  }),

  http.post("*/api/subcategories", async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000003",
      ...body,
    });
  }),

  http.put("*/api/subcategories/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/subcategories/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
