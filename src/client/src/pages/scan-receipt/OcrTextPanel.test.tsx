import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { OcrTextPanel } from "./OcrTextPanel";

const defaultProps = {
  rawText: "Sample OCR text\nSecond line",
  ocrConfidence: 0.87,
};

describe("OcrTextPanel", () => {
  it("renders the collapsible trigger with confidence percentage", () => {
    renderWithProviders(<OcrTextPanel {...defaultProps} />);
    expect(
      screen.getByRole("button", { name: /raw ocr text.*87% confidence/i }),
    ).toBeInTheDocument();
  });

  it("content section is hidden when collapsed (aria-expanded false)", () => {
    renderWithProviders(<OcrTextPanel {...defaultProps} />);
    // Radix keeps CollapsibleContent in the DOM but hides it via CSS;
    // the trigger reflects the collapsed state via aria-expanded.
    const trigger = screen.getByRole("button", {
      name: /raw ocr text.*87% confidence/i,
    });
    expect(trigger).toHaveAttribute("aria-expanded", "false");
  });

  it("expands to show OCR text when trigger is clicked", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OcrTextPanel {...defaultProps} />);

    await user.click(
      screen.getByRole("button", { name: /raw ocr text.*87% confidence/i }),
    );

    // After expanding, the trigger should show aria-expanded="true"
    const trigger = screen.getByRole("button", {
      name: /raw ocr text.*87% confidence/i,
    });
    expect(trigger).toHaveAttribute("aria-expanded", "true");
    // The labeled region should be present
    expect(screen.getByRole("region")).toBeInTheDocument();
  });

  it("collapses when trigger is clicked a second time", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OcrTextPanel {...defaultProps} />);

    const trigger = screen.getByRole("button", {
      name: /raw ocr text.*87% confidence/i,
    });
    await user.click(trigger);
    await user.click(trigger);

    expect(trigger).toHaveAttribute("aria-expanded", "false");
  });

  it("content region has an accessible name via aria-labelledby", async () => {
    const user = userEvent.setup();
    renderWithProviders(<OcrTextPanel {...defaultProps} />);

    await user.click(
      screen.getByRole("button", { name: /raw ocr text.*87% confidence/i }),
    );

    // The section wrapping the OCR text is labeled by the trigger button
    const region = screen.getByRole("region", {
      name: /raw ocr text.*87% confidence/i,
    });
    expect(region).toBeInTheDocument();
    expect(region).toHaveAttribute("aria-labelledby", "ocr-text-trigger");
  });

  it("trigger button has the expected id for aria-labelledby association", () => {
    renderWithProviders(<OcrTextPanel {...defaultProps} />);
    const trigger = screen.getByRole("button", {
      name: /raw ocr text.*87% confidence/i,
    });
    expect(trigger).toHaveAttribute("id", "ocr-text-trigger");
  });

  it("rounds confidence to nearest integer", () => {
    renderWithProviders(
      <OcrTextPanel rawText="text" ocrConfidence={0.555} />,
    );
    expect(
      screen.getByRole("button", { name: /56% confidence/i }),
    ).toBeInTheDocument();
  });
});
