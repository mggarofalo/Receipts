import "@/test/setup-combobox-polyfills";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "@/test/msw/server";
import { categories } from "@/test/msw/fixtures/categories";
import { renderWithQueryClient } from "@/test/test-utils";
import Subcategories from "./Subcategories";

beforeEach(() => {
  localStorage.clear();
});

describe("Subcategories (integration)", () => {
  it("fetches and renders subcategories grouped by category", async () => {
    renderWithQueryClient(<Subcategories />);

    // Category group headers should appear
    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });
    expect(screen.getByText("Tools")).toBeInTheDocument();
  });

  it("shows loading skeleton then resolves to data", async () => {
    const { container } = renderWithQueryClient(<Subcategories />);

    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });
    expect(container.querySelector("[data-slot='skeleton']")).not.toBeInTheDocument();
  });

  it("renders empty state when API returns no data", async () => {
    server.use(
      http.get("*/api/subcategories", () => {
        return HttpResponse.json({ data: [], total: 0, offset: 0, limit: 50 });
      }),
    );

    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText(/no subcategories yet/i)).toBeInTheDocument();
    });
  });

  it("groups are collapsed by default", async () => {
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Subcategory rows should not be visible
    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
    expect(screen.queryByText("Bakery")).not.toBeInTheDocument();
    expect(screen.queryByText("Power Tools")).not.toBeInTheDocument();
  });

  it("clicking a category header expands its subcategories", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Expand Groceries
    await user.click(screen.getByTestId(`category-header-${categories[0].id}`));

    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    // Tools group still collapsed
    expect(screen.queryByText("Power Tools")).not.toBeInTheDocument();
  });

  it("Expand All shows all subcategories", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /expand all/i }));

    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    expect(screen.getByText("Power Tools")).toBeInTheDocument();
  });

  it("Collapse All hides all subcategories", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /expand all/i }));
    expect(screen.getByText("Dairy")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /collapse all/i }));
    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
    expect(screen.queryByText("Power Tools")).not.toBeInTheDocument();
  });

  it("category header shows item count", async () => {
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Groceries has 2 subcategories (Dairy, Bakery)
    const groceriesHeader = screen.getByTestId(`category-header-${categories[0].id}`);
    expect(groceriesHeader).toHaveTextContent("(2)");

    // Tools has 1 subcategory (Power Tools)
    const toolsHeader = screen.getByTestId(`category-header-${categories[1].id}`);
    expect(toolsHeader).toHaveTextContent("(1)");
  });

  it("defaults to showing only active subcategories", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Expand Groceries to see subcategories
    await user.click(screen.getByTestId(`category-header-${categories[0].id}`));

    // Active subcategories visible
    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    // Inactive subcategory filtered out
    expect(screen.queryByText("Expired Coupons")).not.toBeInTheDocument();

    // Active tab selected
    const activeTab = screen.getByRole("tab", { name: "Active" });
    expect(activeTab).toHaveAttribute("data-state", "active");
  });

  it("shows inactive subcategories when Inactive tab is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "Inactive" }));

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Expand Groceries to see inactive subcategories
    await user.click(screen.getByTestId(`category-header-${categories[0].id}`));

    expect(screen.getByText("Expired Coupons")).toBeInTheDocument();
    // Active subcategories should not be shown
    expect(screen.queryByText("Dairy")).not.toBeInTheDocument();
  });

  it("shows all subcategories when All tab is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "All" }));

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Expand Groceries to see all subcategories
    await user.click(screen.getByTestId(`category-header-${categories[0].id}`));

    expect(screen.getByText("Dairy")).toBeInTheDocument();
    expect(screen.getByText("Bakery")).toBeInTheDocument();
    expect(screen.getByText("Expired Coupons")).toBeInTheDocument();
  });

  it("persists status filter in localStorage", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("tab", { name: "All" }));

    expect(localStorage.getItem("subcategories-status-filter")).toBe("all");
  });

  it("opens create dialog and submits a new subcategory via API", async () => {
    const user = userEvent.setup();
    let capturedBody: Record<string, unknown> | null = null;

    server.use(
      http.post("*/api/subcategories", async ({ request }) => {
        capturedBody = (await request.json()) as Record<string, unknown>;
        return HttpResponse.json(
          { id: "new-id", ...capturedBody },
          { status: 201 },
        );
      }),
    );

    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /new subcategory/i }));
    expect(screen.getByRole("heading", { name: /create subcategory/i })).toBeInTheDocument();

    // Name field is a Combobox with allowCustom — open it, type, then use the custom value
    const nameCombobox = screen.getAllByRole("combobox")[0];
    await user.click(nameCombobox);
    const nameInput = screen.getByPlaceholderText("Search names...");
    await user.type(nameInput, "Frozen Foods");
    // Click "Use" button to accept the custom value
    const useButton = await screen.findByText(/Use "Frozen Foods"/);
    await user.click(useButton);

    // Category field is the second Combobox — open it and select Groceries
    const categoryCombobox = screen.getAllByRole("combobox")[1];
    await user.click(categoryCombobox);
    // cmdk items have role="option"
    const groceriesOption = await screen.findByRole("option", { name: /groceries/i });
    await user.click(groceriesOption);

    await user.click(screen.getByRole("button", { name: /create subcategory/i }));

    await waitFor(() => {
      expect(capturedBody).not.toBeNull();
    });
    expect(capturedBody).toMatchObject({ name: "Frozen Foods" });
  });

  it("opens edit dialog when Edit button is clicked after expanding", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByText("Groceries")).toBeInTheDocument();
    });

    // Expand Groceries group
    await user.click(screen.getByTestId(`category-header-${categories[0].id}`));
    expect(screen.getByText("Dairy")).toBeInTheDocument();

    // Click edit on first subcategory
    const editButtons = screen.getAllByRole("button", { name: /edit/i });
    await user.click(editButtons[0]);

    expect(screen.getByRole("heading", { name: /edit subcategory/i })).toBeInTheDocument();
  });

  it("closes create dialog when Cancel is clicked", async () => {
    const user = userEvent.setup();
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /new subcategory/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: /new subcategory/i }));
    expect(screen.getByRole("heading", { name: /create subcategory/i })).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: /cancel/i }));
    await waitFor(() => {
      expect(screen.queryByRole("heading", { name: /create subcategory/i })).not.toBeInTheDocument();
    });
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    renderWithQueryClient(<Subcategories />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /new subcategory/i })).toBeInTheDocument();
    });

    window.dispatchEvent(new Event("shortcut:new-item"));

    await waitFor(() => {
      expect(screen.getByRole("heading", { name: /create subcategory/i })).toBeInTheDocument();
    });
  });

  it("opens create dialog when navigated with openNew state", async () => {
    renderWithQueryClient(<Subcategories />, {
      route: { pathname: "/subcategories", state: { openNew: true } },
    });

    await waitFor(() => {
      expect(
        screen.getByRole("heading", { name: /create subcategory/i }),
      ).toBeInTheDocument();
    });
  });
});
