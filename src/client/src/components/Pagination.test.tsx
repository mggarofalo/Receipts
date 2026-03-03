import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Pagination } from "./Pagination";

describe("Pagination", () => {
  const defaultProps = {
    currentPage: 1,
    totalItems: 50,
    pageSize: 10,
    totalPages: 5,
    onPageChange: vi.fn(),
  };

  it("renders nothing when totalItems is 0", () => {
    const { container } = render(
      <Pagination {...defaultProps} totalItems={0} />,
    );
    expect(container.firstChild).toBeNull();
  });

  it("displays the correct range text", () => {
    render(<Pagination {...defaultProps} />);
    expect(screen.getByText("Showing 1-10 of 50")).toBeInTheDocument();
  });

  it("displays correct range for a middle page", () => {
    render(<Pagination {...defaultProps} currentPage={3} />);
    expect(screen.getByText("Showing 21-30 of 50")).toBeInTheDocument();
  });

  it("displays current page and total pages", () => {
    render(<Pagination {...defaultProps} currentPage={2} />);
    expect(screen.getByText("2/5")).toBeInTheDocument();
  });

  it("disables previous button on first page", () => {
    render(<Pagination {...defaultProps} currentPage={1} />);
    expect(screen.getByLabelText("Previous page")).toBeDisabled();
  });

  it("disables next button on last page", () => {
    render(<Pagination {...defaultProps} currentPage={5} />);
    expect(screen.getByLabelText("Next page")).toBeDisabled();
  });

  it("calls onPageChange with previous page when clicking previous", async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();
    render(
      <Pagination
        {...defaultProps}
        currentPage={3}
        onPageChange={onPageChange}
      />,
    );

    await user.click(screen.getByLabelText("Previous page"));
    expect(onPageChange).toHaveBeenCalledWith(2);
  });

  it("calls onPageChange with next page when clicking next", async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();
    render(
      <Pagination
        {...defaultProps}
        currentPage={3}
        onPageChange={onPageChange}
      />,
    );

    await user.click(screen.getByLabelText("Next page"));
    expect(onPageChange).toHaveBeenCalledWith(4);
  });
});
