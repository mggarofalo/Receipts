import { renderHook, waitFor } from "@testing-library/react";
import { http, HttpResponse } from "msw";
import { server } from "@/test/msw/server";
import { useCards } from "./useCards";
import { createQueryWrapper } from "@/test/test-utils";

describe("useCards (integration)", () => {
  it("returns card list from API", async () => {
    server.use(
      http.get("*/api/cards", () => {
        return HttpResponse.json({
          data: [
            { id: "11111111-1111-1111-1111-111111111111", cardCode: "1000", name: "Cash", isActive: true },
            { id: "22222222-2222-2222-2222-222222222222", cardCode: "2000", name: "Credit Card", isActive: true },
          ],
          total: 2,
          offset: 0,
          limit: 50,
        });
      }),
    );

    const { result } = renderHook(() => useCards(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toHaveLength(2);
    expect(result.current.total).toBe(2);
  });

  it("surfaces API errors", async () => {
    server.use(
      http.get("*/api/cards", () => {
        return HttpResponse.json({ message: "Internal Server Error" }, { status: 500 });
      }),
    );

    const { result } = renderHook(() => useCards(), {
      wrapper: createQueryWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
  });
});
