import { screen } from "@testing-library/react";
import { renderWithQueryClient } from "@/test/test-utils";
import { RecentReceiptsWidget } from "./RecentReceiptsWidget";

vi.mock("@/hooks/useReceipts", () => ({
  useReceipts: vi.fn(),
}));

import { useReceipts } from "@/hooks/useReceipts";
const mockUseReceipts = vi.mocked(useReceipts);

describe("RecentReceiptsWidget", () => {
  it("renders recent receipts", () => {
    mockUseReceipts.mockReturnValue({
      data: {
        data: [
          {
            id: "1",
            location: "Grocery Store",
            date: "2024-01-15",
            taxAmount: 45.99,
            description: null,
          },
          {
            id: "2",
            location: "Gas Station",
            date: "2024-01-14",
            taxAmount: 32.5,
            description: null,
          },
        ],
        total: 2,
        offset: 0,
        limit: 5,
      },
      isLoading: false,
    } as unknown as ReturnType<typeof useReceipts>);

    renderWithQueryClient(<RecentReceiptsWidget />);
    expect(screen.getByText("Grocery Store")).toBeInTheDocument();
    expect(screen.getByText("Gas Station")).toBeInTheDocument();
    expect(screen.getByText("Tax: $45.99")).toBeInTheDocument();
    expect(screen.getByText("Tax: $32.50")).toBeInTheDocument();
  });

  it("renders View all link", () => {
    mockUseReceipts.mockReturnValue({
      data: { data: [], total: 0, offset: 0, limit: 5 },
      isLoading: false,
    } as unknown as ReturnType<typeof useReceipts>);

    renderWithQueryClient(<RecentReceiptsWidget />);
    expect(screen.getByText("View all")).toBeInTheDocument();
  });

  it("shows loading state", () => {
    mockUseReceipts.mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useReceipts>);

    renderWithQueryClient(<RecentReceiptsWidget />);
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
  });

  it("shows empty state when no receipts", () => {
    mockUseReceipts.mockReturnValue({
      data: { data: [], total: 0, offset: 0, limit: 5 },
      isLoading: false,
    } as unknown as ReturnType<typeof useReceipts>);

    renderWithQueryClient(<RecentReceiptsWidget />);
    expect(screen.getByText("No receipts yet")).toBeInTheDocument();
  });
});
