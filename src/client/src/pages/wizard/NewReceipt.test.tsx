import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { mockMutationResult } from "@/test/mock-hooks";
import NewReceipt from "./NewReceipt";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

vi.mock("@/hooks/useReceipts", () => ({
  useCreateReceipt: vi.fn(() => mockMutationResult()),
}));

vi.mock("@/hooks/useTransactions", () => ({
  useCreateTransactionsBatch: vi.fn(() => mockMutationResult()),
}));

vi.mock("@/hooks/useReceiptItems", () => ({
  useCreateReceiptItemsBatch: vi.fn(() => mockMutationResult()),
}));

describe("NewReceipt", () => {
  it("renders the page heading", () => {
    renderWithProviders(<NewReceipt />);
    expect(
      screen.getByRole("heading", { name: /new receipt/i }),
    ).toBeInTheDocument();
  });

  it("renders the wizard stepper", () => {
    renderWithProviders(<NewReceipt />);
    expect(
      screen.getByRole("navigation", { name: /receipt entry progress/i }),
    ).toBeInTheDocument();
  });

  it("renders Step1 (Trip Details) by default", () => {
    renderWithProviders(<NewReceipt />);
    expect(screen.getByLabelText(/location/i)).toBeInTheDocument();
    expect(screen.getByText(/^date$/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toBeInTheDocument();
  });

  it("renders the cancel button", () => {
    renderWithProviders(<NewReceipt />);
    expect(
      screen.getByRole("button", { name: /cancel/i }),
    ).toBeInTheDocument();
  });
});
