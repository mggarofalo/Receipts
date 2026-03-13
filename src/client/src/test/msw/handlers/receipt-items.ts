import { http, HttpResponse } from "msw";
import { receiptItems } from "../fixtures";

export const receiptItemHandlers = [
  http.get("*/api/receipt-items", ({ request }) => {
    const url = new URL(request.url);
    const receiptId = url.searchParams.get("receiptId");
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const filtered = receiptId ? receiptItems.filter((i) => i.receiptId === receiptId) : receiptItems;
    const page = filtered.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: filtered.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/receipt-items/deleted", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    return HttpResponse.json({
      data: [],
      total: 0,
      offset,
      limit,
    });
  }),

  http.get("*/api/receipt-items/:id", ({ params }) => {
    const item = receiptItems.find((i) => i.id === params.id);
    if (!item) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(item);
  }),

  http.post("*/api/receipts/:receiptId/receipt-items", async ({ request, params }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000003",
      receiptId: params.receiptId,
      ...body,
    });
  }),

  http.put("*/api/receipt-items/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/receipt-items/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete("*/api/receipt-items", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post("*/api/receipt-items/:id/restore", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
