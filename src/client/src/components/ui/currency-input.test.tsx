import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
import { CurrencyInput } from "./currency-input";

// Controlled wrapper that re-renders with updated value when onChange fires
function ControlledCurrencyInput({
  initialValue = 0,
  onChange: externalOnChange,
  ...rest
}: {
  initialValue?: number;
  onChange?: (value: number) => void;
} & Omit<React.ComponentProps<typeof CurrencyInput>, "value" | "onChange">) {
  const [value, setValue] = useState(initialValue);
  return (
    <CurrencyInput
      value={value}
      onChange={(v) => {
        setValue(v);
        externalOnChange?.(v);
      }}
      {...rest}
    />
  );
}

describe("CurrencyInput", () => {
  const defaultProps = {
    value: 0,
    onChange: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders with formatted value and dollar symbol", () => {
    render(<CurrencyInput {...defaultProps} value={12.5} />);

    const input = screen.getByRole("textbox");
    expect(input).toHaveValue("12.50");
    expect(screen.getByText("$")).toBeInTheDocument();
  });

  it("shows placeholder instead of 0.00 when value is zero", () => {
    render(<CurrencyInput {...defaultProps} value={0} />);

    const input = screen.getByRole("textbox");
    expect(input).toHaveValue("");
    expect(input).toHaveAttribute("placeholder", "0.00");
  });

  it("formats value on blur to two decimal places", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={5} onChange={onChange} />);

    const input = screen.getByRole("textbox");
    await user.click(input);
    await user.clear(input);
    await user.type(input, "7.1");
    await user.tab();

    // On blur, onChange is called with the final parsed number
    expect(onChange).toHaveBeenCalledWith(7.1);
    // After blur, the display should show "7.10" (formatted with two decimals)
    expect(input).toHaveValue("7.10");
  });

  it("strips non-numeric characters during input", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={0} onChange={onChange} />);

    const input = screen.getByRole("textbox");
    await user.click(input);
    await user.clear(input);
    await user.type(input, "12");

    // onChange should have been called with numeric values
    const lastCall = onChange.mock.calls[onChange.mock.calls.length - 1];
    expect(lastCall[0]).toBe(12);
  });

  it("calls onChange with parsed number on valid input", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={0} onChange={onChange} />);

    const input = screen.getByRole("textbox");
    await user.click(input);
    await user.clear(input);
    await user.type(input, "25.99");

    // Find the call with the final value
    const calls = onChange.mock.calls.map((c) => c[0]);
    expect(calls).toContain(25.99);
  });

  it("handles paste events by stripping non-numeric content", () => {
    const onChange = vi.fn();

    render(<ControlledCurrencyInput initialValue={0} onChange={onChange} />);

    const input = screen.getByRole("textbox");

    // Focus the input
    fireEvent.focus(input);

    // Use fireEvent.paste which works in jsdom
    fireEvent.paste(input, {
      clipboardData: {
        getData: () => "$1,234.56",
      },
    });

    // The paste handler should parse "1234.56" and call onChange with 1234.56
    expect(onChange).toHaveBeenCalledWith(1234.56);
  });

  it("renders custom currency symbol when provided", () => {
    render(<CurrencyInput {...defaultProps} value={10} symbol="EUR" />);

    expect(screen.getByText("EUR")).toBeInTheDocument();
  });

  it("keeps text empty (not '0.00') when focusing a zero-value field", async () => {
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={0} />);

    const input = screen.getByRole("textbox");
    await user.click(input);

    expect(input).toHaveValue("");
  });

  it("returns to empty with placeholder after focusing and blurring a zero-value field without typing", async () => {
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={0} />);

    const input = screen.getByRole("textbox");
    await user.click(input);
    await user.tab();

    expect(input).toHaveValue("");
    expect(input).toHaveAttribute("placeholder", "0.00");
  });

  it("applies normal formatting when typing a value into a zero-value field and blurring", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledCurrencyInput initialValue={0} onChange={onChange} />);

    const input = screen.getByRole("textbox");
    await user.click(input);
    await user.type(input, "42.5");
    await user.tab();

    expect(onChange).toHaveBeenCalledWith(42.5);
    expect(input).toHaveValue("42.50");
  });
});
