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
  useReceiptScan: vi.fn(() => mockMutationResult({ mutate: mockMutate })),
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
        <span data-testid="confidence-map">
          {JSON.stringify(confidenceMap)}
        </span>
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

/** Drives the full scan-success flow and returns the user instance */
async function renderAndScan(storeName = "Test Store") {
  mockMutate.mockImplementation(
    (_file: File, options: { onSuccess: (data: unknown) => void }) => {
      options.onSuccess({
        storeName,
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

  const fileInput = screen.getByTestId("file-input");
  await user.upload(fileInput, createTestFile());
  await user.click(screen.getByRole("button", { name: /scan receipt/i }));

  await waitFor(() => {
    expect(screen.getByTestId("new-receipt-page")).toBeInTheDocument();
  });

  return user;
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

    // Should show processing state — "Processing receipt..." renders in the
    // visible drop-zone paragraph and the sr-only role="status" live region.
    const matches = screen.getAllByText("Processing receipt...");
    expect(matches).toHaveLength(2);
    expect(matches.find((el) => !el.closest(".sr-only"))).toBeInTheDocument();
  });

  it("transitions to review phase on success", async () => {
    await renderAndScan("Test Store");

    expect(screen.getByTestId("prepopulated-location")).toHaveTextContent(
      "Test Store",
    );
  });

  it("shows error on failure", async () => {
    mockMutate.mockImplementation(
      (_file: File, options: { onError: (error: unknown) => void }) => {
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
      expect(screen.getByText(/could not read the file/i)).toBeInTheDocument();
    });
  });

  it("passes correct confidence map when store name confidence is low", async () => {
    mockMutate.mockImplementation(
      (_file: File, options: { onSuccess: (data: unknown) => void }) => {
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

  // ── Phase indicator tests ──────────────────────────────────────────────────

  it("shows the step indicator in scan phase initially", () => {
    renderWithProviders(<ScanReceiptPage />);
    const nav = screen.getByRole("navigation", { name: /scan receipt steps/i });
    expect(nav).toBeInTheDocument();

    // "Scan" step should be aria-current="step"
    const scanStep = screen.getByText("Scan").closest("[aria-current]");
    expect(scanStep).toHaveAttribute("aria-current", "step");

    // "Review" step should NOT be aria-current
    const reviewStep = screen.getByText("Review").closest("li");
    expect(reviewStep).not.toHaveAttribute("aria-current");
  });

  it("marks the review step as aria-current after successful scan", async () => {
    await renderAndScan();

    const reviewStep = screen.getByText("Review").closest("[aria-current]");
    expect(reviewStep).toHaveAttribute("aria-current", "step");

    // Scan step should no longer be current
    const scanStep = screen.getByText("Scan").closest("li");
    expect(scanStep).not.toHaveAttribute("aria-current");
  });

  // ── Live region announcement tests ────────────────────────────────────────

  it("announces phase transition in a live region after successful scan", async () => {
    await renderAndScan();

    await waitFor(() => {
      const region = screen.getByTestId("phase-announcement");
      expect(region).toHaveTextContent("Receipt scanned, review details below");
    });
  });

  it("live region has role=status and aria-live=polite", () => {
    renderWithProviders(<ScanReceiptPage />);
    const region = screen.getByTestId("phase-announcement");
    expect(region).toHaveAttribute("role", "status");
    expect(region).toHaveAttribute("aria-live", "polite");
    expect(region).toHaveAttribute("aria-atomic", "true");
  });

  // ── Focus management tests ─────────────────────────────────────────────────

  it("moves focus to the review heading after successful scan", async () => {
    await renderAndScan();

    const heading = screen.getByTestId("review-heading");
    expect(heading).toBeInTheDocument();
    expect(heading).toHaveFocus();
  });

  it("review heading is focusable programmatically via tabIndex=-1", async () => {
    await renderAndScan();

    const heading = screen.getByTestId("review-heading");
    expect(heading).toHaveAttribute("tabindex", "-1");
  });
});
