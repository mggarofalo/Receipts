import { http, HttpResponse } from "msw";
import { scanProposal } from "../fixtures/scan";

export const scanHandlers = [
  http.post("*/api/receipts/scan", () => {
    return HttpResponse.json(scanProposal);
  }),
];
