import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import AdminUsers from "./AdminUsers";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useAuth", () => ({
  useAuth: vi.fn(() => ({
    user: { email: "admin@example.com" },
  })),
}));

vi.mock("@/hooks/useUsers", () => ({
  useUsers: vi.fn(() => ({
    data: { items: [], totalCount: 0 },
    isLoading: false,
  })),
  useCreateUser: vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false })),
  useUpdateUser: vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false })),
  useDeleteUser: vi.fn(() => ({ mutateAsync: vi.fn(), isPending: false })),
  useResetUserPassword: vi.fn(() => ({
    mutateAsync: vi.fn(),
    isPending: false,
  })),
}));

vi.mock("@/hooks/useListKeyboardNav", () => ({
  useListKeyboardNav: vi.fn(() => ({
    focusedId: null,
    setFocusedIndex: vi.fn(),
    tableRef: { current: null },
  })),
}));

describe("AdminUsers", () => {
  it("renders the page heading", () => {
    renderWithProviders(<AdminUsers />);
    expect(
      screen.getByRole("heading", { name: /user management/i }),
    ).toBeInTheDocument();
  });

  it("renders the Create User button", () => {
    renderWithProviders(<AdminUsers />);
    expect(
      screen.getByRole("button", { name: /create user/i }),
    ).toBeInTheDocument();
  });

  it("renders loading skeleton when data is loading", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: true,
    } as ReturnType<typeof useUsers>);

    const { container } = renderWithProviders(<AdminUsers />);
    expect(container.querySelector("[data-slot='skeleton']")).toBeInTheDocument();
  });

  it("renders empty state when no users exist", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: { items: [], totalCount: 0 },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    expect(screen.getByText(/no users found/i)).toBeInTheDocument();
  });

  it("renders user table when users exist", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            roles: ["Admin"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    expect(screen.getByText("test@example.com")).toBeInTheDocument();
  });

  it("opens create user dialog when Create User button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    renderWithProviders(<AdminUsers />);

    await user.click(screen.getByRole("button", { name: /create user/i }));

    expect(
      screen.getByText(/create a new user account/i),
    ).toBeInTheDocument();
  });

  it("opens edit user dialog when Edit button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            roles: ["Admin"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    await user.click(screen.getByRole("button", { name: /edit/i }));

    expect(
      screen.getByText(/update user details/i),
    ).toBeInTheDocument();
  });

  it("opens reset password dialog when Reset PW button is clicked", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            roles: ["Admin"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    await user.click(screen.getByRole("button", { name: /reset pw/i }));

    expect(
      screen.getByRole("heading", { name: /reset password/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/set a new password/i),
    ).toBeInTheDocument();
  });

  it("disables Disable button for the current user", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "admin@example.com",
            firstName: "Admin",
            lastName: "User",
            roles: ["Admin"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    expect(screen.getByRole("button", { name: /disable/i })).toBeDisabled();
  });

  it("disables Disable button for already disabled users", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "2",
            email: "disabled@example.com",
            firstName: "Disabled",
            lastName: "User",
            roles: ["User"],
            isDisabled: true,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    expect(screen.getByRole("button", { name: /disable/i })).toBeDisabled();
    expect(screen.getByText("Disabled")).toBeInTheDocument();
  });

  it("shows user name formatted from first and last name", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            roles: ["Admin"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: "2024-01-15",
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    expect(screen.getByText("John Doe")).toBeInTheDocument();
  });

  it("shows dash when user has no first or last name", async () => {
    const { useUsers } = await import("@/hooks/useUsers");
    vi.mocked(useUsers).mockReturnValue({
      data: {
        items: [
          {
            id: "1",
            email: "test@example.com",
            firstName: null,
            lastName: null,
            roles: ["User"],
            isDisabled: false,
            createdAt: "2024-01-01",
            lastLoginAt: null,
          },
        ],
        totalCount: 1,
      },
      isLoading: false,
    } as ReturnType<typeof useUsers>);

    renderWithProviders(<AdminUsers />);
    // The name column should show "-"
    const cells = screen.getAllByRole("cell");
    expect(cells[0].textContent).toBe("-");
  });
});
