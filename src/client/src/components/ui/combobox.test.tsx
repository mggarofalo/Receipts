import { describe, it, expect, vi, beforeAll, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Combobox, type ComboboxOption } from "./combobox";

// Polyfill ResizeObserver and scrollIntoView for radix-ui / cmdk in jsdom
beforeAll(() => {
  if (typeof window.ResizeObserver === "undefined") {
    window.ResizeObserver = class {
      observe() {}
      unobserve() {}
      disconnect() {}
    } as unknown as typeof ResizeObserver;
  }

  // cmdk calls scrollIntoView on list items, which jsdom doesn't implement
  if (!Element.prototype.scrollIntoView) {
    Element.prototype.scrollIntoView = vi.fn();
  }
});

const options: ComboboxOption[] = [
  { value: "apple", label: "Apple" },
  { value: "banana", label: "Banana" },
  { value: "cherry", label: "Cherry" },
];

describe("Combobox", () => {
  const defaultProps = {
    options,
    value: "",
    onValueChange: vi.fn(),
    placeholder: "Select fruit...",
    searchPlaceholder: "Search fruit...",
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders with placeholder when no value is selected", () => {
    render(<Combobox {...defaultProps} />);

    expect(screen.getByRole("combobox")).toHaveTextContent("Select fruit...");
  });

  it("displays selected option label when value matches an option", () => {
    render(<Combobox {...defaultProps} value="banana" />);

    expect(screen.getByRole("combobox")).toHaveTextContent("Banana");
  });

  it("opens popover on click and shows options", async () => {
    const user = userEvent.setup();
    render(<Combobox {...defaultProps} />);

    const trigger = screen.getByRole("combobox");
    await user.click(trigger);

    await waitFor(() => {
      expect(screen.getByText("Apple")).toBeInTheDocument();
      expect(screen.getByText("Banana")).toBeInTheDocument();
      expect(screen.getByText("Cherry")).toBeInTheDocument();
    });
  });

  it("selects an option and calls onValueChange", async () => {
    const onValueChange = vi.fn();
    const user = userEvent.setup();

    render(<Combobox {...defaultProps} onValueChange={onValueChange} />);

    await user.click(screen.getByRole("combobox"));

    await waitFor(() => {
      expect(screen.getByText("Cherry")).toBeInTheDocument();
    });

    await user.click(screen.getByText("Cherry"));

    expect(onValueChange).toHaveBeenCalledWith("cherry");
  });

  it("shows loading text when loading prop is true", () => {
    render(<Combobox {...defaultProps} loading />);

    expect(screen.getByRole("combobox")).toHaveTextContent("Loading...");
  });

  it("shows raw value when value does not match any option", () => {
    render(<Combobox {...defaultProps} value="custom-value" />);

    expect(screen.getByRole("combobox")).toHaveTextContent("custom-value");
  });
});
