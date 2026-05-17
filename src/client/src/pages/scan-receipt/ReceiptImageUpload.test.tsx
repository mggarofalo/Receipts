import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { ReceiptImageUpload } from "./ReceiptImageUpload";

function createTestFile(
  name = "receipt.jpg",
  type = "image/jpeg",
  sizeBytes = 1024,
): File {
  const buffer = new ArrayBuffer(sizeBytes);
  return new File([buffer], name, { type });
}

describe("ReceiptImageUpload", () => {
  const defaultProps = {
    onScan: vi.fn(),
    isLoading: false,
    error: null,
  };

  beforeEach(() => {
    vi.clearAllMocks();
    // Mock URL.createObjectURL / revokeObjectURL
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

  it("renders the drop zone", () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);
    expect(
      screen.getByLabelText("Drop zone for receipt file"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Drop a receipt image or PDF here"),
    ).toBeInTheDocument();
  });

  it("drop zone has aria-describedby pointing at always-present format hint", () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);
    const dropZone = screen.getByLabelText("Drop zone for receipt file");
    expect(dropZone).toHaveAttribute("aria-describedby", "drop-zone-hint");
    // The hint element must exist in the DOM even when a file has been selected
    const hint = document.getElementById("drop-zone-hint");
    expect(hint).toBeInTheDocument();
    expect(hint).toHaveTextContent(/JPEG, PNG, or PDF/i);
    expect(hint).toHaveTextContent(/20 MB/i);
  });

  it("format hint remains in DOM after a file is selected", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const fileInput = screen.getByTestId("file-input");
    await user.upload(fileInput, createTestFile());

    // hint must still exist so aria-describedby keeps working
    expect(document.getElementById("drop-zone-hint")).toBeInTheDocument();
  });

  it("renders choose file and scan buttons", () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /choose file/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /scan receipt/i }),
    ).toBeInTheDocument();
  });

  it("shows preview when a file is selected via file picker", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);

    expect(screen.getByAltText("Receipt preview")).toBeInTheDocument();
    expect(screen.getByText("receipt.jpg")).toBeInTheDocument();
  });

  it("calls onScan with the selected file when Scan Receipt is clicked", async () => {
    const onScan = vi.fn();
    const user = userEvent.setup();
    renderWithProviders(
      <ReceiptImageUpload {...defaultProps} onScan={onScan} />,
    );

    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile();
    await user.upload(fileInput, file);

    await user.click(screen.getByRole("button", { name: /scan receipt/i }));
    expect(onScan).toHaveBeenCalledWith(file);
  });

  it("shows loading state when isLoading is true", () => {
    renderWithProviders(
      <ReceiptImageUpload {...defaultProps} isLoading={true} />,
    );
    // "Processing receipt..." appears in both the visible paragraph and the sr-only
    // live region; use getAllByText to assert at least one visible instance exists.
    const matches = screen.getAllByText("Processing receipt...");
    expect(matches.length).toBeGreaterThanOrEqual(1);
  });

  it("polite live region announces processing state", () => {
    renderWithProviders(
      <ReceiptImageUpload {...defaultProps} isLoading={true} />,
    );
    const statusRegion = document.querySelector("[role='status']");
    expect(statusRegion).toBeInTheDocument();
    expect(statusRegion).toHaveAttribute("aria-live", "polite");
    expect(statusRegion).toHaveTextContent("Processing receipt...");
  });

  it("polite live region announces selected filename after file is chosen", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const fileInput = screen.getByTestId("file-input");
    await user.upload(fileInput, createTestFile("my-receipt.jpg"));

    const statusRegion = document.querySelector("[role='status']");
    expect(statusRegion).toHaveTextContent("File selected: my-receipt.jpg");
  });

  it("polite live region is empty when no file is selected and not loading", () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);
    const statusRegion = document.querySelector("[role='status']");
    expect(statusRegion).toBeInTheDocument();
    expect(statusRegion).toHaveTextContent("");
  });

  it("shows error alert when error prop is provided", () => {
    renderWithProviders(
      <ReceiptImageUpload
        {...defaultProps}
        error="Could not read the image"
      />,
    );
    expect(
      screen.getByText("Could not read the image"),
    ).toBeInTheDocument();
  });

  it("error alert is wrapped in assertive live region", () => {
    renderWithProviders(
      <ReceiptImageUpload {...defaultProps} error="Could not read the image" />,
    );
    const assertiveRegion = document.querySelector("[aria-live='assertive']");
    expect(assertiveRegion).toBeInTheDocument();
    expect(assertiveRegion).toHaveAttribute("aria-atomic", "true");
    expect(assertiveRegion).toHaveTextContent("Could not read the image");
  });

  it("validation error renders inside assertive live region", async () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const dropZone = screen.getByLabelText("Drop zone for receipt file");
    const file = createTestFile(
      "document.docx",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    );

    const dataTransfer = {
      files: [file],
      items: [{ kind: "file", type: file.type, getAsFile: () => file }],
      types: ["Files"],
    };

    const { fireEvent } = await import("@testing-library/react");
    fireEvent.drop(dropZone, { dataTransfer });

    const assertiveRegion = document.querySelector("[aria-live='assertive']");
    expect(assertiveRegion).toHaveTextContent(
      "Only JPEG, PNG, and PDF files are supported.",
    );
  });

  it("validates file type and rejects unsupported files via drag-and-drop", async () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const dropZone = screen.getByLabelText("Drop zone for receipt file");
    const file = createTestFile("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

    const dataTransfer = {
      files: [file],
      items: [{ kind: "file", type: file.type, getAsFile: () => file }],
      types: ["Files"],
    };

    // fireEvent.drop triggers the handleDrop callback
    const { fireEvent } = await import("@testing-library/react");
    fireEvent.drop(dropZone, { dataTransfer });

    expect(
      screen.getByText("Only JPEG, PNG, and PDF files are supported."),
    ).toBeInTheDocument();
  });

  it("accepts PDF files via drag-and-drop", async () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const dropZone = screen.getByLabelText("Drop zone for receipt file");
    const file = createTestFile("receipt.pdf", "application/pdf");

    const dataTransfer = {
      files: [file],
      items: [{ kind: "file", type: file.type, getAsFile: () => file }],
      types: ["Files"],
    };

    const { fireEvent } = await import("@testing-library/react");
    fireEvent.drop(dropZone, { dataTransfer });

    // Should show the file name (PDF preview uses icon, not img)
    expect(screen.getByText("receipt.pdf")).toBeInTheDocument();
    // Should NOT show a validation error
    expect(screen.queryByText(/only JPEG/i)).not.toBeInTheDocument();
  });

  it("shows PDF icon for PDF files instead of image preview", async () => {
    const user = userEvent.setup();
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);

    const fileInput = screen.getByTestId("file-input");
    const file = createTestFile("receipt.pdf", "application/pdf");
    await user.upload(fileInput, file);

    // Should show file name but not an image preview
    expect(screen.getByText("receipt.pdf")).toBeInTheDocument();
    expect(screen.queryByAltText("Receipt preview")).not.toBeInTheDocument();
  });

  it("disables scan button when no file is selected", () => {
    renderWithProviders(<ReceiptImageUpload {...defaultProps} />);
    const actual = screen.getByRole("button", { name: /scan receipt/i });
    expect(actual).toBeDisabled();
  });
});
