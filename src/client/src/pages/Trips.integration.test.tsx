import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse, delay } from "msw";
import { server } from "@/test/msw/server";
import { renderWithQueryClient } from "@/test/test-utils";
import Trips from "./Trips";

describe("Trips page integration", () => {
  it("renders receipt table with data from MSW handler", async () => {
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Grocery Store")).toBeInTheDocument();
    });

    expect(screen.getByText("Hardware Store")).toBeInTheDocument();
    expect(screen.getByText("Restaurant")).toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Location" })).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Date" })).toBeInTheDocument();
    expect(screen.getByRole("columnheader", { name: "Tax Amount" })).toBeInTheDocument();
  });

  it("selecting a receipt loads trip data", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Grocery Store")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Grocery Store"));

    await waitFor(() => {
      expect(screen.getByText("Milk")).toBeInTheDocument();
    });

    expect(screen.getByText("Bread")).toBeInTheDocument();
    expect(screen.getAllByText("$17.73").length).toBeGreaterThan(0);
    expect(screen.getByText("Cash")).toBeInTheDocument();
  });

  it('shows "No trip found" when API returns 404', async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Trips />);

    await waitFor(() => {
      expect(screen.getByText("Restaurant")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Restaurant"));

    await waitFor(() => {
      expect(screen.getByText("No trip found for this receipt.")).toBeInTheDocument();
    });
  });

  it("handles loading states correctly", async () => {
    server.use(
      http.get("*/api/receipts", async () => {
        await delay(200);
        return HttpResponse.json({
          data: [{ id: "aaaa1111-1111-1111-1111-111111111111", location: "Delayed Store", date: "2025-01-15", taxAmount: 1.0 }],
          total: 1,
          offset: 0,
          limit: 50,
        });
      }),
    );

    renderWithQueryClient(<Trips />);

    expect(document.querySelector("[data-slot='skeleton']")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Delayed Store")).toBeInTheDocument();
    });
  });
});
