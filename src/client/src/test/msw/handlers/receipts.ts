import { http, HttpResponse } from "msw";
import { receipts } from "../fixtures";

export const receiptHandlers = [
  http.get("*/api/receipts", ({ request }) => {
    const url = new URL(request.url);
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const page = receipts.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: receipts.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/receipts/deleted", ({ request }) => {
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

  http.get("*/api/receipts/:id", ({ params }) => {
    const receipt = receipts.find((r) => r.id === params.id);
    if (!receipt) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(receipt);
  }),

  http.post("*/api/receipts", async ({ request }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000002",
      ...body,
    });
  }),

  http.put("*/api/receipts/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete("*/api/receipts", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post("*/api/receipts/:id/restore", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
