import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderWithProviders } from "@/test/test-utils";
import { ActiveFilterBanner } from "./ActiveFilterBanner";

describe("ActiveFilterBanner", () => {
  it("displays the message text", () => {
    renderWithProviders(
      <ActiveFilterBanner message="Filtered by vendor" onClear={vi.fn()} />,
    );

    expect(screen.getByText("Filtered by vendor")).toBeInTheDocument();
  });

  it("calls onClear when 'Clear filter' button is clicked", async () => {
    const user = userEvent.setup();
    const onClear = vi.fn();

    renderWithProviders(
      <ActiveFilterBanner message="Filtered by vendor" onClear={onClear} />,
    );

    await user.click(screen.getByRole("button", { name: /clear filter/i }));

    expect(onClear).toHaveBeenCalledOnce();
  });
});
