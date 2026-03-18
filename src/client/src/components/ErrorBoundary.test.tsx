import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ErrorBoundary } from "./ErrorBoundary";

function ThrowingChild({ shouldThrow }: { shouldThrow: boolean }) {
  if (shouldThrow) {
    throw new Error("Test error message");
  }
  return <div>Child rendered successfully</div>;
}

describe("ErrorBoundary", () => {
  // Suppress console.error for expected errors during tests
  const originalConsoleError = console.error;
  beforeEach(() => {
    console.error = vi.fn();
  });
  afterEach(() => {
    console.error = originalConsoleError;
  });

  it("renders children when no error is thrown", () => {
    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={false} />
      </ErrorBoundary>,
    );
    expect(screen.getByText("Child rendered successfully")).toBeInTheDocument();
  });

  it("renders default error UI when a child throws", () => {
    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
    expect(
      screen.getByText("An unexpected error occurred. Please try again."),
    ).toBeInTheDocument();
    expect(screen.getByText("Test error message")).toBeInTheDocument();
  });

  it("renders custom fallback when provided and a child throws", () => {
    render(
      <ErrorBoundary fallback={<div>Custom fallback</div>}>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    expect(screen.getByText("Custom fallback")).toBeInTheDocument();
    expect(screen.queryByText("Something went wrong")).not.toBeInTheDocument();
  });

  it("renders Try Again and Go Home buttons in default error UI", () => {
    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    expect(screen.getByRole("button", { name: /try again/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /go home/i })).toBeInTheDocument();
  });

  it("displays the error message in the default error UI", () => {
    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    // The error message "Test error message" should be displayed in a pre element
    const errorMessage = screen.getByText("Test error message");
    expect(errorMessage.tagName).toBe("PRE");
  });

  it("has role=alert on the error UI container", () => {
    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    expect(screen.getByRole("alert")).toBeInTheDocument();
  });

  it("resets error state when Try Again is clicked", async () => {
    const user = userEvent.setup();
    // Use a ref-like variable that the child reads at render time
    let shouldThrow = true;
    function ConditionalChild() {
      if (shouldThrow) throw new Error("Test error message");
      return <div>Child rendered successfully</div>;
    }

    render(
      <ErrorBoundary>
        <ConditionalChild />
      </ErrorBoundary>,
    );
    // Error UI is showing
    expect(screen.getByText("Something went wrong")).toBeInTheDocument();
    // Fix the child so it won't throw on next render
    shouldThrow = false;
    // Click Try Again — this calls handleReset which clears the error state
    await user.click(screen.getByRole("button", { name: /try again/i }));
    expect(screen.getByText("Child rendered successfully")).toBeInTheDocument();
    expect(screen.queryByText("Something went wrong")).not.toBeInTheDocument();
  });

  it("navigates to home when Go Home button is clicked", async () => {
    const user = userEvent.setup();
    const assignMock = vi.fn();
    Object.defineProperty(window, "location", {
      value: { ...window.location, assign: assignMock },
      writable: true,
    });

    render(
      <ErrorBoundary>
        <ThrowingChild shouldThrow={true} />
      </ErrorBoundary>,
    );
    await user.click(screen.getByRole("button", { name: /go home/i }));
    expect(assignMock).toHaveBeenCalledWith("/");
  });
});
