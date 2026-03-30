import "@/test/setup-combobox-polyfills";
import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { Step1TripDetails } from "./Step1TripDetails";

vi.mock("@/hooks/useReceipts", () => ({
  useLocationSuggestions: vi.fn(() => ({ data: undefined })),
}));

describe("Step1TripDetails", () => {
  const defaultProps = {
    data: { location: "", date: "", taxAmount: 0 },
    onNext: vi.fn(),
  };

  beforeEach(() => {
    localStorage.clear();
  });

  it("renders the form fields", () => {
    renderWithProviders(<Step1TripDetails {...defaultProps} />);
    expect(screen.getByRole("combobox")).toBeInTheDocument();
    expect(screen.getByText(/^date$/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toBeInTheDocument();
    expect(screen.getByLabelText(/tax amount/i)).toBeInTheDocument();
  });

  it("renders with pre-filled data", () => {
    const data = { location: "Costco", date: "2024-03-15", taxAmount: 7.5 };
    renderWithProviders(<Step1TripDetails {...defaultProps} data={data} />);
    // Combobox shows raw value as text content when no matching option exists
    expect(screen.getByRole("combobox")).toHaveTextContent("Costco");
    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toHaveValue("03/15/2024");
  });

  it("renders the Next button", () => {
    renderWithProviders(<Step1TripDetails {...defaultProps} />);
    expect(screen.getByRole("button", { name: /next/i })).toBeInTheDocument();
  });

  it("calls onNext when form is submitted with valid data", async () => {
    const user = (await import("@testing-library/user-event")).default.setup();
    const onNext = vi.fn();
    renderWithProviders(
      <Step1TripDetails
        {...defaultProps}
        onNext={onNext}
        data={{ location: "Walmart", date: "2024-01-15", taxAmount: 3 }}
      />,
    );

    await user.click(screen.getByRole("button", { name: /next/i }));
    await vi.waitFor(() => {
      expect(onNext).toHaveBeenCalledWith({
        location: "Walmart",
        date: "2024-01-15",
        taxAmount: 3,
      });
    });
  });

  it("renders the card title", () => {
    renderWithProviders(<Step1TripDetails {...defaultProps} />);
    expect(screen.getByText("Trip Details")).toBeInTheDocument();
  });
});
