import { describe, it, expect, vi } from "vitest";
import { render, screen, act } from "@testing-library/react";
import { useContext } from "react";
import { ShortcutsProvider } from "./ShortcutsContext";
import { ShortcutsContext } from "./shortcuts-context";

vi.mock("react-hotkeys-hook", () => ({
  HotkeysProvider: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="hotkeys-provider">{children}</div>
  ),
}));

function TestConsumer() {
  const ctx = useContext(ShortcutsContext);
  if (!ctx) return <div>no context</div>;
  return (
    <div>
      <span data-testid="help-open">{String(ctx.helpOpen)}</span>
      <button
        data-testid="toggle"
        onClick={() => ctx.setHelpOpen(!ctx.helpOpen)}
      />
      <button data-testid="set-true" onClick={() => ctx.setHelpOpen(true)} />
    </div>
  );
}

describe("ShortcutsProvider", () => {
  it("provides helpOpen=false initially", () => {
    render(
      <ShortcutsProvider>
        <TestConsumer />
      </ShortcutsProvider>,
    );

    expect(screen.getByTestId("help-open")).toHaveTextContent("false");
  });

  it("setHelpOpen toggles the value", () => {
    render(
      <ShortcutsProvider>
        <TestConsumer />
      </ShortcutsProvider>,
    );

    expect(screen.getByTestId("help-open")).toHaveTextContent("false");

    act(() => {
      screen.getByTestId("toggle").click();
    });

    expect(screen.getByTestId("help-open")).toHaveTextContent("true");

    act(() => {
      screen.getByTestId("toggle").click();
    });

    expect(screen.getByTestId("help-open")).toHaveTextContent("false");
  });

  it("renders children within HotkeysProvider", () => {
    render(
      <ShortcutsProvider>
        <div data-testid="child">Hello</div>
      </ShortcutsProvider>,
    );

    const hotkeysWrapper = screen.getByTestId("hotkeys-provider");
    const child = screen.getByTestId("child");
    expect(hotkeysWrapper).toContainElement(child);
    expect(child).toHaveTextContent("Hello");
  });

  it("setHelpOpen accepts explicit boolean values", () => {
    render(
      <ShortcutsProvider>
        <TestConsumer />
      </ShortcutsProvider>,
    );

    act(() => {
      screen.getByTestId("set-true").click();
    });

    expect(screen.getByTestId("help-open")).toHaveTextContent("true");

    // Clicking set-true again still keeps it true
    act(() => {
      screen.getByTestId("set-true").click();
    });

    expect(screen.getByTestId("help-open")).toHaveTextContent("true");
  });
});
