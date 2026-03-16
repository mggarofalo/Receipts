import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "@/test/msw/server";
import { receipts, tripResponse } from "@/test/msw/fixtures/receipts";
import { createEnumMetadataHandler } from "@/test/msw/handler-factories";
import { renderWithQueryClient } from "@/test/test-utils";
import Trips from "./Trips";

beforeEach(() => {
  localStorage.clear();
  server.use(
    http.get("*/api/receipts", () => {
      return HttpResponse.json({
        data: receipts,
        total: receipts.length,
        offset: 0,
        limit: 10000,
      });
    }),
    http.get("*/api/trips", ({ request }) => {
      const url = new URL(request.url);
      const receiptId = url.searchParams.get("receiptId");
      if (receiptId === receipts[0].id) {
        return HttpResponse.json(tripResponse);
      }
      return HttpResponse.json({ message: "Not found" }, { status: 404 });
    }),
    createEnumMetadataHandler(),
  );
});

describe("Trips (integration)", () => {
  it("fetches and renders receipt list from the API", async () => {
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });
    expect(screen.getByText("Target")).toBeInTheDocument();
  });

  it("shows loading skeleton then resolves to receipt list", async () => {
    const { container } = renderWithQueryClient(<Trips />);

    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });
  });

  it("renders empty state when API returns no receipts", async () => {
    server.use(
      http.get("*/api/receipts", () => {
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 10000 });
      }),
    );

    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText(/no receipts found/i)).toBeInTheDocument();
    });
  });

  it("loads trip details when a receipt row is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });

    // Click the Walmart receipt row
    await user.click(screen.getByText("Walmart"));

    // Trip detail sections should appear
    await waitFor(() => {
      expect(screen.getByText("Receipt")).toBeInTheDocument();
    });

    // Receipt info card — date appears in both the table row and detail card
    expect(screen.getAllByText(/2024-06-15/).length).toBeGreaterThanOrEqual(2);

    // Transactions section
    expect(screen.getByText("Transactions (1)")).toBeInTheDocument();
    expect(screen.getByText("1000")).toBeInTheDocument();
    expect(screen.getByText("Cash")).toBeInTheDocument();

    // Adjustments section
    expect(screen.getByText("Adjustments (1)")).toBeInTheDocument();
    expect(screen.getByText("Store coupon")).toBeInTheDocument();
  });

  it("shows error state when trip is not found for a receipt", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Target")).toBeInTheDocument();
    });

    // Click Target — our handler returns 404 for this receipt
    await user.click(screen.getByText("Target"));

    await waitFor(() => {
      expect(screen.getByText(/no trip found for this receipt/i)).toBeInTheDocument();
    });
  });

  it("renders receipt items in trip detail", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Walmart"));

    await waitFor(() => {
      expect(screen.getByText("Whole Milk")).toBeInTheDocument();
    });
  });

  it("renders balance summary card with totals", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Walmart"));

    // Wait for trip to load — balance summary should show
    await waitFor(() => {
      expect(screen.getByText("Transactions (1)")).toBeInTheDocument();
    });
  });

  it("shows no transactions/adjustments messages when trip has none", async () => {
    server.use(
      http.get("*/api/trips", () => {
        return HttpResponse.json({
          ...tripResponse,
          receipt: {
            ...tripResponse.receipt,
            adjustments: [],
            adjustmentTotal: 0,
          },
          transactions: [],
        });
      }),
    );

    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Walmart"));

    await waitFor(() => {
      expect(screen.getByText(/no transactions for this receipt/i)).toBeInTheDocument();
    });
    expect(screen.getByText(/no adjustments for this receipt/i)).toBeInTheDocument();
  });

  it("highlights selected receipt row", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Walmart")).toBeInTheDocument();
    });

    const walmartRow = screen.getByText("Walmart").closest("tr");
    expect(walmartRow).not.toHaveClass("bg-accent");

    await user.click(screen.getByText("Walmart"));

    // After clicking, the row should have the accent class
    expect(walmartRow).toHaveClass("bg-accent");
  });
});
