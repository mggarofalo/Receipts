import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { DateRangeSelector } from "./DateRangeSelector";
import type { DateRange } from "@/hooks/useDashboard";

const defaultRange: DateRange = {
  startDate: "2024-01-01",
  endDate: "2024-01-31",
};

describe("DateRangeSelector", () => {
  it("renders all preset buttons", () => {
    const onChange = vi.fn();
    renderWithProviders(
      <DateRangeSelector value={defaultRange} onChange={onChange} />,
    );
    expect(screen.getByRole("button", { name: "7 days" })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "30 days" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "90 days" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Year to date" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "All time" }),
    ).toBeInTheDocument();
  });

  it("renders custom date button", () => {
    const onChange = vi.fn();
    renderWithProviders(
      <DateRangeSelector value={defaultRange} onChange={onChange} />,
    );
    expect(
      screen.getByRole("button", { name: /custom/i }),
    ).toBeInTheDocument();
  });

  it("calls onChange when a preset is clicked", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    renderWithProviders(
      <DateRangeSelector value={defaultRange} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: "7 days" }));
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        startDate: expect.any(String),
        endDate: expect.any(String),
      }),
    );
  });

  it("calls onChange with early start date for All time", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    renderWithProviders(
      <DateRangeSelector value={defaultRange} onChange={onChange} />,
    );

    await user.click(screen.getByRole("button", { name: "All time" }));
    expect(onChange).toHaveBeenCalledWith(
      expect.objectContaining({
        startDate: "2000-01-01",
        endDate: expect.any(String),
      }),
    );
  });

  it("renders a dropdown selector for narrow viewports", () => {
    const onChange = vi.fn();
    renderWithProviders(
      <DateRangeSelector value={defaultRange} onChange={onChange} />,
    );
    expect(screen.getByRole("combobox")).toBeInTheDocument();
  });
});
