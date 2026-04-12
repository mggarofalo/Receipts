import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { mockMutationResult } from "@/test/mock-hooks";
import ScanReceiptPage from "./ScanReceiptPage";

vi.mock("@/hooks/usePageTitle", () => ({
  usePageTitle: vi.fn(),
}));

const mockMutate = vi.fn();

vi.mock("@/hooks/useReceiptScan", () => ({
  useReceiptScan: vi.fn(() =>
    mockMutationResult({ mutate: mockMutate }),
  ),
}));

// Mock NewReceiptPage to isolate ScanReceiptPage logic
vi.mock("@/pages/new-receipt/NewReceiptPage", () => ({
  default: ({
    initialValues,
    confidenceMap,
  }: {
    initialValues?: { header: { location: string } };
    confidenceMap?: Record<string, string>;
  }) => (
    <div data-testid="new-receipt-page">
      {initialValues?.header.location && (
        <span data-testid="prepopulated-location">
          {initialValues.header.location}
        </span>
      )}
      {confidenceMap && (
        <span data-testid="confidence-map">{JSON.stringify(confidenceMap)}</span>
      )}
    </div>
  ),
}));

function createTestFile(
  name = "receipt.jpg",
  type = "image/jpeg",
  sizeBytes = 1024,
): File {
  const buffer = new ArrayBuffer(sizeBytes);
  return new File([buffer], name, { type });
}

describe("ScanReceiptPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.stubGlobal(
      "URL",
      Object.assign({}, globalThis.URL, {
        createObjectURL: vi.fn(() => "blob:preview-url"),
        revokeObjectURL: vi.fn(),
      }),
    );
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("renders the upload phase initially", () => {
    renderWithProviders(<ScanReceiptPage />);
    expect(
      screen.getByRole("heading", { name: /scan receipt/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Drop a receipt image or PDF here"),
    ).toBeInTheDocument();
  });

  it("shows loading state during scan", async () => {
    // Make mutate do nothing (simulate pending state)
    mockMutate.mockImplementation(() => {});

    const user = userEvent.setup();
    renderWithProviders(<ScanReceiptPage />);

    // Select a file
    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);

    // Click scan
    await user.click(screen.getByRole("button", { name: /scan receipt/i }));

    // Should show processing state
    expect(screen.getByText("Processing receipt...")).toBeInTheDocument();
  });

  it("transitions to review phase on success", async () => {
    mockMutate.mockImplementation(
      (
        _file: File,
        options: { onSuccess: (data: unknown) => void },
      ) => {
        options.onSuccess({
          storeName: "Test Store",
          storeNameConfidence: "high",
          date: "2024-06-15",
          dateConfidence: "high",
          items: [],
          subtotal: 0,
          subtotalConfidence: "high",
          taxLines: [
            {
              label: "Tax",
              labelConfidence: "high",
              amount: 0,
              amountConfidence: "high",
            },
          ],
          total: 0,
          totalConfidence: "high",
          paymentMethod: null,
          paymentMethodConfidence: "high",
          rawOcrText: "TEST STORE\nTotal: 0.00",
          ocrConfidence: 0.9,
        });
      },
    );

    const user = userEvent.setup();
    renderWithProviders(<ScanReceiptPage />);

    // Select a file
    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);

    // Click scan
    await user.click(screen.getByRole("button", { name: /scan receipt/i }));

    await waitFor(() => {
      expect(screen.getByTestId("new-receipt-page")).toBeInTheDocument();
    });

    expect(screen.getByTestId("prepopulated-location")).toHaveTextContent(
      "Test Store",
    );
  });

  it("shows error on failure", async () => {
    mockMutate.mockImplementation(
      (
        _file: File,
        options: { onError: (error: unknown) => void },
      ) => {
        options.onError({ status: 400 });
      },
    );

    const user = userEvent.setup();
    renderWithProviders(<ScanReceiptPage />);

    // Select a file
    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);

    // Click scan
    await user.click(screen.getByRole("button", { name: /scan receipt/i }));

    await waitFor(() => {
      expect(
        screen.getByText(/could not read the file/i),
      ).toBeInTheDocument();
    });
  });

  it("passes correct confidence map when store name confidence is low", async () => {
    mockMutate.mockImplementation(
      (
        _file: File,
        options: { onSuccess: (data: unknown) => void },
      ) => {
        options.onSuccess({
          storeName: "Low Confidence Store",
          storeNameConfidence: "low",
          date: "2024-06-15",
          dateConfidence: "high",
          items: [],
          subtotal: 10,
          subtotalConfidence: "high",
          taxLines: [
            {
              label: "Tax",
              labelConfidence: "high",
              amount: 1,
              amountConfidence: "high",
            },
          ],
          total: 11,
          totalConfidence: "high",
          paymentMethod: null,
          paymentMethodConfidence: "high",
          rawOcrText: "LOW CONFIDENCE STORE",
          ocrConfidence: 0.5,
        });
      },
    );

    const user = userEvent.setup();
    renderWithProviders(<ScanReceiptPage />);

    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);
    await user.click(screen.getByRole("button", { name: /scan receipt/i }));

    await waitFor(() => {
      expect(screen.getByTestId("confidence-map")).toBeInTheDocument();
    });

    const confidenceMap = JSON.parse(
      screen.getByTestId("confidence-map").textContent!,
    );
    expect(confidenceMap).toEqual({ location: "low" });
  });
});
