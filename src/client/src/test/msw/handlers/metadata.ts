import { http, HttpResponse } from "msw";
import { enumMetadata } from "../fixtures";

export const metadataHandlers = [
  http.get("*/api/metadata/enums", () => {
    return HttpResponse.json(enumMetadata);
  }),
];
