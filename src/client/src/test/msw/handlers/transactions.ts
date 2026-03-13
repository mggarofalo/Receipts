import { http, HttpResponse } from "msw";
import { transactions } from "../fixtures";

export const transactionHandlers = [
  http.get("*/api/transactions", ({ request }) => {
    const url = new URL(request.url);
    const receiptId = url.searchParams.get("receiptId");
    const offset = Number(url.searchParams.get("offset") ?? 0);
    const limit = Number(url.searchParams.get("limit") ?? 50);
    const filtered = receiptId ? transactions.filter((t) => t.receiptId === receiptId) : transactions;
    const page = filtered.slice(offset, offset + limit);
    return HttpResponse.json({
      data: page,
      total: filtered.length,
      offset,
      limit,
    });
  }),

  http.get("*/api/transactions/deleted", ({ request }) => {
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

  http.get("*/api/transactions/:id", ({ params }) => {
    const txn = transactions.find((t) => t.id === params.id);
    if (!txn) return HttpResponse.json({ message: "Not Found" }, { status: 404 });
    return HttpResponse.json(txn);
  }),

  http.post("*/api/receipts/:receiptId/transactions", async ({ request, params }) => {
    const body = (await request.json()) as Record<string, unknown>;
    return HttpResponse.json({
      id: "00000000-0000-0000-0000-000000000004",
      receiptId: params.receiptId,
      ...body,
    });
  }),

  http.put("*/api/transactions/:id", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.put("*/api/transactions/batch", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.delete("*/api/transactions", () => {
    return new HttpResponse(null, { status: 204 });
  }),

  http.post("*/api/transactions/:id/restore", () => {
    return new HttpResponse(null, { status: 204 });
  }),
];
