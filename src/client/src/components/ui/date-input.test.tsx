import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { createRef, useState } from "react";
import { DateInput } from "./date-input";

// Controlled wrapper that re-renders with updated value when onChange fires
function ControlledDateInput({
  initialValue = "",
  onChange: externalOnChange,
  ...rest
}: {
  initialValue?: string;
  onChange?: (value: string) => void;
} & Omit<React.ComponentProps<typeof DateInput>, "value" | "onChange">) {
  const [value, setValue] = useState(initialValue);
  return (
    <DateInput
      value={value}
      onChange={(v) => {
        setValue(v);
        externalOnChange?.(v);
      }}
      {...rest}
    />
  );
}

describe("DateInput", () => {
  const defaultProps = {
    value: "",
    onChange: vi.fn(),
  };

  let originalMatchMedia: typeof window.matchMedia;

  beforeEach(() => {
    vi.clearAllMocks();
    originalMatchMedia = window.matchMedia;
    // Default: desktop (pointer: fine) — existing tests rely on this
    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      media: "(pointer: coarse)",
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    });
  });

  afterEach(() => {
    window.matchMedia = originalMatchMedia;
  });

  it("renders with placeholder when value is empty", () => {
    render(<DateInput {...defaultProps} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(input).toBeInTheDocument();
    expect(input).toHaveValue("");
  });

  it("renders with formatted display value when given a wire-format date", () => {
    render(<DateInput {...defaultProps} value="2024-03-15" />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(input).toHaveValue("03/15/2024");
  });

  it("renders the calendar button", () => {
    render(<DateInput {...defaultProps} />);

    expect(
      screen.getByRole("button", { name: "Pick a date" }),
    ).toBeInTheDocument();
  });

  it("calls onChange with wire format when user types a valid MM/DD/YYYY date and blurs", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "03/15/2024");
    await user.tab();

    expect(onChange).toHaveBeenCalledWith("2024-03-15");
  });

  it("calls onChange with wire format when user types YYYY-MM-DD and blurs", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "2024-01-20");
    await user.tab();

    expect(onChange).toHaveBeenCalledWith("2024-01-20");
  });

  it("formats the display value on blur after valid input", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "2024-06-01");
    await user.tab();

    // After blur, the display should show MM/DD/YYYY format
    expect(input).toHaveValue("06/01/2024");
  });

  it("clears the value when user clears the input and blurs", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(
      <ControlledDateInput initialValue="2024-03-15" onChange={onChange} />,
    );

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.clear(input);
    await user.tab();

    expect(onChange).toHaveBeenCalledWith("");
    expect(input).toHaveValue("");
  });

  it("disables both input and calendar button when disabled", () => {
    render(<DateInput {...defaultProps} disabled />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(input).toBeDisabled();

    const calendarButton = screen.getByRole("button", { name: "Pick a date" });
    expect(calendarButton).toBeDisabled();
  });

  it("passes aria-required through to the input", () => {
    render(<DateInput {...defaultProps} aria-required="true" />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(input).toHaveAttribute("aria-required", "true");
  });

  it("updates display when controlled value changes externally", () => {
    const { rerender } = render(
      <DateInput value="2024-01-01" onChange={vi.fn()} />,
    );

    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toHaveValue("01/01/2024");

    rerender(<DateInput value="2024-12-25" onChange={vi.fn()} />);

    expect(screen.getByPlaceholderText("MM/DD/YYYY")).toHaveValue("12/25/2024");
  });

  it("supports custom placeholder via props", () => {
    render(<DateInput {...defaultProps} placeholder="Select date" />);

    expect(screen.getByPlaceholderText("Select date")).toBeInTheDocument();
  });

  it("calls onBlur when the input loses focus", async () => {
    const onBlur = vi.fn();
    const user = userEvent.setup();

    render(<DateInput {...defaultProps} onBlur={onBlur} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.tab();

    expect(onBlur).toHaveBeenCalledTimes(1);
  });

  // --- New tests for PR review fixes ---

  it("does not eagerly fire onChange while typing (no intermediate dates)", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<DateInput {...defaultProps} onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "03/1");

    // onChange should not be called while still typing partial text
    expect(onChange).not.toHaveBeenCalled();
  });

  it("opens calendar and selects a date", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(
      <ControlledDateInput
        initialValue="2024-03-15"
        onChange={onChange}
      />,
    );

    const calendarButton = screen.getByRole("button", { name: /pick a date/i });
    await user.click(calendarButton);

    // Calendar should be open — select March 20th, 2024 by its full aria-label
    const dayButton = await screen.findByRole("button", {
      name: /Wednesday, March 20th, 2024/,
    });
    await user.click(dayButton);

    expect(onChange).toHaveBeenCalledWith("2024-03-20");
  });

  it("rejects date beyond max prop and shows error state", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(
      <ControlledDateInput
        initialValue=""
        onChange={onChange}
        max="2024-01-15"
      />,
    );

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "02/01/2024");
    await user.tab();

    // Date is after max, so onChange should NOT have been called with the date
    expect(onChange).not.toHaveBeenCalledWith("2024-02-01");
    // Input should show invalid state
    expect(input).toHaveAttribute("aria-invalid", "true");
  });

  it("accepts date at or before max prop", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(
      <ControlledDateInput
        initialValue=""
        onChange={onChange}
        max="2024-06-15"
      />,
    );

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "06/15/2024");
    await user.tab();

    expect(onChange).toHaveBeenCalledWith("2024-06-15");
    expect(input).not.toHaveAttribute("aria-invalid");
  });

  it("rejects date before min prop and shows error state", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(
      <ControlledDateInput
        initialValue=""
        onChange={onChange}
        min="2024-06-01"
      />,
    );

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "01/15/2024");
    await user.tab();

    // Date is before min, so onChange should NOT have been called with the date
    expect(onChange).not.toHaveBeenCalledWith("2024-01-15");
    // Input should show invalid state
    expect(input).toHaveAttribute("aria-invalid", "true");
  });

  it("shows error state for unparseable input on blur", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<DateInput {...defaultProps} onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "not-a-date");
    await user.tab();

    // onChange should not have been called with a date value
    expect(onChange).not.toHaveBeenCalledWith(
      expect.stringMatching(/^\d{4}-\d{2}-\d{2}$/),
    );
    // Input should be marked invalid
    expect(input).toHaveAttribute("aria-invalid", "true");
  });

  it("clears error state when valid date is committed", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");

    // First type garbage
    await user.click(input);
    await user.type(input, "garbage");
    await user.tab();
    expect(input).toHaveAttribute("aria-invalid", "true");

    // Now type a valid date
    await user.click(input);
    await user.clear(input);
    await user.type(input, "03/15/2024");
    await user.tab();
    expect(input).not.toHaveAttribute("aria-invalid");
  });

  it("forwards ref to the input element", () => {
    const ref = createRef<HTMLInputElement>();
    render(<DateInput {...defaultProps} ref={ref} />);

    expect(ref.current).toBeInstanceOf(HTMLInputElement);
    expect(ref.current).toBe(screen.getByPlaceholderText("MM/DD/YYYY"));
  });

  it("uses inputMode='text' not 'numeric'", () => {
    render(<DateInput {...defaultProps} />);
    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    expect(input).toHaveAttribute("inputMode", "text");
  });

  it("calendar trigger button has aria-expanded", () => {
    render(<DateInput {...defaultProps} />);
    const button = screen.getByRole("button", { name: /pick a date/i });
    expect(button).toHaveAttribute("aria-expanded");
  });

  it("calendar trigger button has adequate touch target size (size-11)", () => {
    render(<DateInput {...defaultProps} />);
    const button = screen.getByRole("button", { name: /pick a date/i });
    // size-11 = 44px (2.75rem), check the class is present
    expect(button.className).toContain("size-11");
  });

  it("rejects ambiguous partial matches via round-trip check", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(<DateInput {...defaultProps} onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "03/1");
    fireEvent.blur(input);

    // Should not have committed a date — partial input fails round-trip
    expect(onChange).not.toHaveBeenCalledWith(
      expect.stringMatching(/^\d{4}-\d{2}-\d{2}$/),
    );
  });

  it("commits valid date on Enter key", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "03/15/2024");
    await user.keyboard("{Enter}");

    expect(onChange).toHaveBeenCalledWith("2024-03-15");
  });

  it("reformats display value after Enter key commit", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "2024-06-01");
    await user.keyboard("{Enter}");

    // After Enter, blur is triggered so display should show MM/DD/YYYY format
    expect(input).toHaveValue("06/01/2024");
  });

  it("does not fire redundant onChange when commitText value matches current value", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(
      <ControlledDateInput initialValue="2024-03-15" onChange={onChange} />,
    );

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    // Focus then blur without changing the value
    await user.click(input);
    await user.tab();

    // onChange should NOT have been called since value didn't change
    expect(onChange).not.toHaveBeenCalled();
  });

  it("does not fire redundant onChange on calendar pick followed by blur", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();
    render(
      <ControlledDateInput
        initialValue="2024-03-15"
        onChange={onChange}
      />,
    );

    const calendarButton = screen.getByRole("button", { name: /pick a date/i });
    await user.click(calendarButton);

    // Select March 20th, 2024
    const dayButton = await screen.findByRole("button", {
      name: /Wednesday, March 20th, 2024/,
    });
    await user.click(dayButton);

    // onChange fired once for the calendar pick
    expect(onChange).toHaveBeenCalledTimes(1);
    expect(onChange).toHaveBeenCalledWith("2024-03-20");

    // Now focus and blur the input — should NOT fire onChange again
    onChange.mockClear();
    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.tab();

    expect(onChange).not.toHaveBeenCalled();
  });

  describe("touch / mobile path", () => {
    let originalMatchMedia: typeof window.matchMedia;

    beforeEach(() => {
      originalMatchMedia = window.matchMedia;
      window.matchMedia = vi.fn().mockReturnValue({
        matches: true,
        media: "(pointer: coarse)",
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        dispatchEvent: vi.fn(),
      });
    });

    afterEach(() => {
      window.matchMedia = originalMatchMedia;
    });

    it("renders a native date input on touch devices", () => {
      render(<DateInput {...defaultProps} />);

      const input = screen.getByDisplayValue("");
      expect(input).toHaveAttribute("type", "date");
    });

    it("does not render the Popover/Calendar button on touch devices", () => {
      render(<DateInput {...defaultProps} />);

      expect(
        screen.queryByRole("button", { name: "Pick a date" }),
      ).not.toBeInTheDocument();
    });

    it("passes value through as wire format", () => {
      render(<DateInput {...defaultProps} value="2024-03-15" />);

      const input = screen.getByDisplayValue("2024-03-15");
      expect(input).toHaveAttribute("type", "date");
    });

    it("calls onChange with the native input value", () => {
      const onChange = vi.fn();
      render(<DateInput value="" onChange={onChange} />);

      const input = document.querySelector('input[type="date"]')!;
      fireEvent.change(input, { target: { value: "2024-06-15" } });

      expect(onChange).toHaveBeenCalledWith("2024-06-15");
    });

    it("passes min and max to the native input", () => {
      render(
        <DateInput
          {...defaultProps}
          min="2024-01-01"
          max="2024-12-31"
        />,
      );

      const input = document.querySelector('input[type="date"]')!;
      expect(input).toHaveAttribute("min", "2024-01-01");
      expect(input).toHaveAttribute("max", "2024-12-31");
    });

    it("disables the native input when disabled", () => {
      render(<DateInput {...defaultProps} disabled />);

      const input = document.querySelector('input[type="date"]')!;
      expect(input).toBeDisabled();
    });

    it("fires onBlur on the native input", () => {
      const onBlur = vi.fn();
      render(<DateInput {...defaultProps} onBlur={onBlur} />);

      const input = document.querySelector('input[type="date"]')!;
      fireEvent.blur(input);

      expect(onBlur).toHaveBeenCalledTimes(1);
    });

    it("forwards ref to the native date input", () => {
      const ref = createRef<HTMLInputElement>();
      render(<DateInput {...defaultProps} ref={ref} />);

      expect(ref.current).toBeInstanceOf(HTMLInputElement);
      expect(ref.current?.type).toBe("date");
    });
  });
});
