import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { FuzzySearchInput } from "./FuzzySearchInput";

describe("FuzzySearchInput", () => {
  const defaultProps = {
    value: "",
    onChange: vi.fn(),
  };

  it("renders the input with default placeholder", () => {
    render(<FuzzySearchInput {...defaultProps} />);
    expect(screen.getByPlaceholderText("Search...")).toBeInTheDocument();
  });

  it("renders with custom placeholder", () => {
    render(
      <FuzzySearchInput {...defaultProps} placeholder="Find items..." />,
    );
    expect(screen.getByPlaceholderText("Find items...")).toBeInTheDocument();
  });

  it("calls onChange when user types in the input", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<FuzzySearchInput {...defaultProps} onChange={onChange} />);

    await user.type(screen.getByPlaceholderText("Search..."), "a");
    expect(onChange).toHaveBeenCalledWith("a");
  });

  it("shows result count badge when value is present and counts are provided", () => {
    render(
      <FuzzySearchInput
        {...defaultProps}
        value="test"
        resultCount={5}
        totalCount={20}
      />,
    );
    expect(screen.getByText("5/20")).toBeInTheDocument();
  });

  it("does not show result count badge when value is empty", () => {
    render(
      <FuzzySearchInput
        {...defaultProps}
        value=""
        resultCount={5}
        totalCount={20}
      />,
    );
    expect(screen.queryByText("5/20")).not.toBeInTheDocument();
  });

  it("shows clear button when value is non-empty and clears on click", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(
      <FuzzySearchInput
        {...defaultProps}
        value="test"
        onChange={onChange}
      />,
    );

    const clearButton = screen.getByLabelText("Clear search");
    await user.click(clearButton);
    expect(onChange).toHaveBeenCalledWith("");
  });

  it("shows keyboard shortcut hint when showShortcutHint is true and value is empty", () => {
    render(
      <FuzzySearchInput {...defaultProps} showShortcutHint />,
    );
    expect(screen.getByText("Ctrl+K")).toBeInTheDocument();
  });

  it("hides keyboard shortcut hint when value is present", () => {
    render(
      <FuzzySearchInput {...defaultProps} value="test" showShortcutHint />,
    );
    expect(screen.queryByText("Ctrl+K")).not.toBeInTheDocument();
  });
});
