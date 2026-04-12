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
    expect(screen.getByText("Processing receipt...")).toBeInTheDocument();
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
