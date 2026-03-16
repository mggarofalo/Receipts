import { http, HttpResponse } from "msw";
import { tripResponse } from "../fixtures";

export const tripHandlers = [
  http.get("*/api/trips", ({ request }) => {
    const url = new URL(request.url);
    const receiptId = url.searchParams.get("receiptId");
    if (!receiptId) {
      return HttpResponse.json({ message: "receiptId is required" }, { status: 400 });
    }
    if (receiptId === tripResponse.receipt.receipt.id) {
      return HttpResponse.json(tripResponse);
    }
    return HttpResponse.json({ message: "Not Found" }, { status: 404 });
  }),
];
