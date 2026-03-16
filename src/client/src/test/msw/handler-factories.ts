import { http, HttpResponse, type HttpHandler } from "msw";

/**
 * Creates standard list + CRUD handlers for a paginated API entity.
 *
 * Pattern: GET /api/{resource} returns { data, total, offset, limit }
 *          POST /api/{resource} creates and returns the entity
 *          PUT /api/{resource}/:id updates and returns 204
 */
export function createListHandlers<T extends { id: string }>(
  resource: string,
  items: T[],
): HttpHandler[] {
  return [
    http.get(`*/api/${resource}`, ({ request }) => {
      const url = new URL(request.url);
      const offset = Number(url.searchParams.get("offset") ?? 0);
      const limit = Number(url.searchParams.get("limit") ?? 50);
      const page = items.slice(offset, offset + limit);
      return HttpResponse.json({
        data: page,
        total: items.length,
        offset,
        limit,
      });
    }),
    http.post(`*/api/${resource}`, async ({ request }) => {
      const body = (await request.json()) as Partial<T>;
      return HttpResponse.json(
        { id: "new-id-1111-1111-1111-111111111111", ...body },
        { status: 201 },
      );
    }),
    http.put(`*/api/${resource}/:id`, () => {
      return new HttpResponse(null, { status: 204 });
    }),
  ];
}

/**
 * Creates a handler for the enum metadata endpoint.
 * Many pages depend on this (Trips, ItemTemplates via forms).
 */
export function createEnumMetadataHandler(): HttpHandler {
  return http.get("*/api/metadata/enums", () => {
    return HttpResponse.json({
      adjustmentTypes: [
        { value: "Discount", label: "Discount" },
        { value: "Fee", label: "Fee" },
      ],
      authEventTypes: [],
      pricingModes: [
        { value: "quantity", label: "Quantity" },
        { value: "flat", label: "Flat" },
      ],
      auditActions: [],
      entityTypes: [],
    });
  });
}
