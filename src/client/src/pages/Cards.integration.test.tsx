import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "@/test/msw/server";
import { cards } from "@/test/msw/fixtures/cards";
import { createListHandlers } from "@/test/msw/handler-factories";
import { renderWithQueryClient } from "@/test/test-utils";
import Cards from "./Cards";

beforeEach(() => {
  localStorage.clear();
  server.use(...createListHandlers("cards", cards));
});

describe("Cards (integration)", () => {
  it("fetches and renders cards from the API", async () => {
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });
    expect(screen.getByText("Credit Card")).toBeInTheDocument();
    expect(screen.getByText("1000")).toBeInTheDocument();
    expect(screen.getByText("2000")).toBeInTheDocument();
  });

  it("shows loading skeleton then resolves to data", async () => {
    const { container } = renderWithQueryClient(<Cards />);

    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });
    expect(container.querySelector("[data-slot='skeleton']")).not.toBeInTheDocument();
  });

  it("renders empty state when API returns no data", async () => {
    server.use(
      http.get("*/api/cards", () => {
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 50 });
      }),
    );

    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText(/no cards yet/i)).toBeInTheDocument();
    });
  });

  it("defaults to showing only active cards", async () => {
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    expect(screen.getByText("Credit Card")).toBeInTheDocument();
    expect(screen.queryByText("Closed Savings")).not.toBeInTheDocument();

    const activeTab = screen.getByRole("tab", { name: "Active" });
    expect(activeTab).toHaveAttribute("data-state", "active");
  });

  it("shows inactive cards when Inactive tab is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "Inactive" }));

    expect(screen.queryByText("Cash")).not.toBeInTheDocument();
    expect(screen.getByText("Closed Savings")).toBeInTheDocument();
  });

  it("shows all cards when All tab is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "All" }));

    expect(screen.getByText("Cash")).toBeInTheDocument();
    expect(screen.getByText("Credit Card")).toBeInTheDocument();
    expect(screen.getByText("Closed Savings")).toBeInTheDocument();
  });

  it("persists status filter in localStorage", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "All" }));

    expect(localStorage.getItem("cards-status-filter")).toBe("all");
  });

  it("opens create dialog and submits a new card via API", async () => {
    const user = userEvent.setup();
    let capturedBody: Record<string, unknown> | null = null;

    server.use(
      http.post("*/api/cards", async ({ request }) => {
        capturedBody = (await request.json()) as Record<string, unknown>;
        return HttpResponse.json(
          { id: "new-id", ...capturedBody },
          { status: 201 },
        );
      }),
    );

    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /new card/i }));
    expect(screen.getByRole("heading", { name: /create card/i })).toBeInTheDocument();

    await user.type(screen.getByLabelText(/card code/i), "4000");
    await user.type(screen.getByLabelText(/^name$/i), "New Checking");
    await user.click(screen.getByRole("button", { name: /create card/i }));

    await waitFor(() => {
      expect(capturedBody).not.toBeNull();
    });
    expect(capturedBody).toMatchObject({
      cardCode: "4000",
      name: "New Checking",
      isActive: true,
    });
  });

  it("opens edit dialog and submits updates via API", async () => {
    const user = userEvent.setup();
    let capturedBody: Record<string, unknown> | null = null;

    server.use(
      http.put("*/api/cards/:id", async ({ request }) => {
        capturedBody = (await request.json()) as Record<string, unknown>;
        return new HttpResponse(null, { status: 204 });
      }),
    );

    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByText("Cash")).toBeInTheDocument();
    });

    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);

    expect(screen.getByRole("heading", { name: /edit card/i })).toBeInTheDocument();

    const nameInput = screen.getByLabelText(/^name$/i);
    await user.clear(nameInput);
    await user.type(nameInput, "Updated Cash");
    await user.click(screen.getByRole("button", { name: /update card/i }));

    await waitFor(() => {
      expect(capturedBody).not.toBeNull();
    });
    expect(capturedBody).toMatchObject({ name: "Updated Cash" });
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /new card/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /new card/i }));
    expect(screen.getByRole("heading", { name: /create card/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create card/i })).not.toBeInTheDocument();
    });
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    renderWithQueryClient(<Cards />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /new card/i })).toBeInTheDocument();
    });

    window.dispatchEvent(new Event("shortcut:new-item"));

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: /create card/i })).toBeInTheDocument();
    });
  });
});
