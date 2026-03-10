import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { useState } from "react";
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

  beforeEach(() => {
    vi.clearAllMocks();
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

  it("calls onChange with wire format when user types a valid MM/DD/YYYY date", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "03/15/2024");

    // onChange should have been called with the wire format at some point
    const calls = onChange.mock.calls.map((c) => c[0]);
    expect(calls).toContain("2024-03-15");
  });

  it("calls onChange with wire format when user types YYYY-MM-DD", async () => {
    const onChange = vi.fn();
    const user = userEvent.setup();

    render(<ControlledDateInput initialValue="" onChange={onChange} />);

    const input = screen.getByPlaceholderText("MM/DD/YYYY");
    await user.click(input);
    await user.type(input, "2024-01-20");

    const calls = onChange.mock.calls.map((c) => c[0]);
    expect(calls).toContain("2024-01-20");
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
});
