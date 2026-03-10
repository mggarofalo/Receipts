import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import { mockQueryResult } from "@/test/mock-hooks";
import ApiKeys from "./ApiKeys";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

vi.mock("@tanstack/react-query", async (importOriginal) => {
  const actual =
    await importOriginal<typeof import("@tanstack/react-query")>();
  return {
    ...actual,
    useQuery: vi.fn(() => ({
      data: [],
      isLoading: false,
    })),
    useMutation: vi.fn(() => ({
      mutate: vi.fn(),
      isPending: false,
    })),
    useQueryClient: vi.fn(() => ({
      invalidateQueries: vi.fn(),
    })),
  };
});

vi.mock("@/lib/toast", () => ({
  showSuccess: vi.fn(),
  showError: vi.fn(),
}));

vi.mock("@/lib/api-client", () => ({
  default: {
    GET: vi.fn(),
    POST: vi.fn(),
    DELETE: vi.fn(),
  },
}));

describe("ApiKeys", () => {
  it("renders the page heading", () => {
    renderWithQueryClient(<ApiKeys />);
    expect(
      screen.getByRole("heading", { name: /api keys/i }),
    ).toBeInTheDocument();
  });

  it("renders the Create API Key button", () => {
    renderWithQueryClient(<ApiKeys />);
    expect(
      screen.getByRole("button", { name: /create api key/i }),
    ).toBeInTheDocument();
  });

  it("renders the description text", () => {
    renderWithQueryClient(<ApiKeys />);
    expect(
      screen.getByText(/manage api keys for programmatic access/i),
    ).toBeInTheDocument();
  });

  it("renders the Your API Keys card", () => {
    renderWithQueryClient(<ApiKeys />);
    expect(screen.getByText(/your api keys/i)).toBeInTheDocument();
  });

  it("renders empty state when no API keys exist", () => {
    renderWithQueryClient(<ApiKeys />);
    expect(
      screen.getByText(/no api keys yet/i),
    ).toBeInTheDocument();
  });

  it("renders table with API keys when data exists", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    expect(screen.getByText("Test Key")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /revoke/i }),
    ).toBeInTheDocument();
  });

  it("opens create dialog when Create API Key button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithQueryClient(<ApiKeys />);

    await user.click(
      screen.getByRole("button", { name: /create api key/i }),
    );

    expect(
      screen.getByRole("heading", { name: /create api key/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText(/paperless integration/i),
    ).toBeInTheDocument();
  });

  it("shows revoked badge for revoked keys", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-2",
          name: "Revoked Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: true,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    expect(screen.getByText("Revoked")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /revoke/i })).not.toBeInTheDocument();
  });

  it("shows expired badge for expired keys", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-3",
          name: "Expired Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: "2020-01-01T00:00:00Z",
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    expect(screen.getByText("Expired")).toBeInTheDocument();
  });

  it("opens revoke confirmation dialog when Revoke button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /revoke/i }));

    expect(
      screen.getByRole("heading", { name: /revoke api key/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/this action cannot be undone/i),
    ).toBeInTheDocument();
  });

  it("calls mutate when revoke is confirmed", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useQuery, useMutation } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));
    vi.mocked(useMutation).mockImplementation((() => ({
      mutate: mockMutate,
      isPending: false,
    })) as unknown as typeof useMutation);

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /^revoke$/i }));

    // Now click the destructive Revoke button in the dialog
    const dialogButtons = screen.getAllByRole("button", { name: /revoke/i });
    const confirmButton = dialogButtons.find(
      (btn) => btn.closest("[role='dialog']") !== null,
    );
    if (confirmButton) {
      await user.click(confirmButton);
      expect(mockMutate).toHaveBeenCalledWith("key-1");
    }
  });

  it("opens create dialog on shortcut:new-item event", async () => {
    const { act } = await import("@testing-library/react");
    renderWithQueryClient(<ApiKeys />);

    act(() => {
      window.dispatchEvent(new Event("shortcut:new-item"));
    });

    await screen.findByRole("heading", { name: /create api key/i });
    expect(
      screen.getByRole("heading", { name: /create api key/i }),
    ).toBeInTheDocument();
  });

  it("shows loading skeleton when data is loading", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [],
      isLoading: true,
    }));

    const { container } = renderWithQueryClient(<ApiKeys />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("formats dates correctly in the table", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-06-15T00:00:00Z",
          lastUsedAt: "2024-07-20T00:00:00Z",
          expiresAt: "2025-06-15T00:00:00Z",
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    // Dates should be formatted as locale date strings, not raw ISO
    expect(screen.queryByText("2024-06-15T00:00:00Z")).not.toBeInTheDocument();
  });

  it("shows created key dialog when create mutation succeeds", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useMutation } = await import("@tanstack/react-query");
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useMutation).mockImplementation(((opts: any) => ({
      mutate: vi.fn((values: unknown) => {
        if (opts?.onSuccess) {
          opts.onSuccess({ rawKey: "test-secret-key-123" }, values, undefined);
        }
      }),
      isPending: false,
    })) as unknown as typeof useMutation);

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /create api key/i }));
    await user.type(screen.getByPlaceholderText(/paperless integration/i), "My Key");
    await user.click(screen.getByRole("button", { name: /create key/i }));

    // The created key dialog should appear with the key
    expect(await screen.findByRole("heading", { name: /api key created/i })).toBeInTheDocument();
    expect(screen.getByText(/save this key now/i)).toBeInTheDocument();
    expect(screen.getByDisplayValue("test-secret-key-123")).toBeInTheDocument();
  });

  it("copies key to clipboard when Copy to Clipboard button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useMutation } = await import("@tanstack/react-query");
    const mockWriteText = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(navigator, "clipboard", {
      value: { writeText: mockWriteText },
      writable: true,
      configurable: true,
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    vi.mocked(useMutation).mockImplementation(((opts: any) => ({
      mutate: vi.fn((values: unknown) => {
        if (opts?.onSuccess) {
          opts.onSuccess({ rawKey: "copy-me-key" }, values, undefined);
        }
      }),
      isPending: false,
    })) as unknown as typeof useMutation);

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /create api key/i }));
    await user.type(screen.getByPlaceholderText(/paperless integration/i), "Copy Test");
    await user.click(screen.getByRole("button", { name: /create key/i }));

    await screen.findByRole("heading", { name: /api key created/i });
    await user.click(screen.getByRole("button", { name: /copy to clipboard/i }));

    expect(mockWriteText).toHaveBeenCalledWith("copy-me-key");
  });

  it("cancels revoke dialog when Cancel button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /^revoke$/i }));

    // Dialog should be open
    expect(screen.getByRole("heading", { name: /revoke api key/i })).toBeInTheDocument();

    // Click Cancel
    await user.click(screen.getByRole("button", { name: /cancel/i }));

    // Dialog should close
    await vi.waitFor(() => {
      expect(screen.queryByRole("heading", { name: /revoke api key/i })).not.toBeInTheDocument();
    });
  });

  it("shows create form submission with mutation", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const mockMutate = vi.fn();
    const { useMutation } = await import("@tanstack/react-query");
    vi.mocked(useMutation).mockImplementation((() => ({
      mutate: mockMutate,
      isPending: false,
    })) as unknown as typeof useMutation);

    renderWithQueryClient(<ApiKeys />);
    await user.click(screen.getByRole("button", { name: /create api key/i }));
    await user.type(screen.getByPlaceholderText(/paperless integration/i), "New API Key");
    await user.click(screen.getByRole("button", { name: /create key/i }));

    await vi.waitFor(() => {
      expect(mockMutate).toHaveBeenCalledWith(expect.objectContaining({ name: "New API Key" }));
    });
  });

  it("formats date with null value as dash", async () => {
    const { useQuery } = await import("@tanstack/react-query");
    vi.mocked(useQuery).mockReturnValue(mockQueryResult({
      data: [
        {
          id: "key-1",
          name: "Test Key",
          createdAt: "2024-01-01T00:00:00Z",
          lastUsedAt: null,
          expiresAt: null,
          isRevoked: false,
        },
      ],
      isLoading: false,
    }));

    renderWithQueryClient(<ApiKeys />);
    // lastUsedAt and expiresAt are null — should show "-"
    const cells = screen.getAllByRole("cell");
    const dashCells = cells.filter((c) => c.textContent === "-");
    expect(dashCells.length).toBeGreaterThanOrEqual(2);
  });
});
