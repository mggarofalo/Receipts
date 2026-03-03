import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { ShortcutsContext } from "@/contexts/shortcuts-context";
import { ShortcutsHelp } from "./ShortcutsHelp";

vi.mock("@/lib/shortcut-registry", () => ({
  getShortcutsByCategory: vi.fn(() => {
    const map = new Map();
    map.set("Global", [
      { keys: "?", label: "Show keyboard shortcuts", category: "Global" },
      { keys: "Ctrl+K", label: "Open command palette", category: "Global" },
    ]);
    map.set("Forms", [
      { keys: "Ctrl+Enter", label: "Submit form", category: "Forms" },
    ]);
    return map;
  }),
}));

function renderWithShortcutsContext(
  helpOpen: boolean,
  setHelpOpen = vi.fn(),
) {
  return render(
    <ShortcutsContext.Provider value={{ helpOpen, setHelpOpen }}>
      <ShortcutsHelp />
    </ShortcutsContext.Provider>,
  );
}

describe("ShortcutsHelp", () => {
  it("renders nothing when context is not provided", () => {
    const { container } = render(<ShortcutsHelp />);
    expect(container.firstChild).toBeNull();
  });

  it("renders the dialog title when open", () => {
    renderWithShortcutsContext(true);
    expect(screen.getByText("Keyboard Shortcuts")).toBeInTheDocument();
  });

  it("renders shortcut categories as section headings", () => {
    renderWithShortcutsContext(true);
    expect(screen.getByText("Global")).toBeInTheDocument();
    expect(screen.getByText("Forms")).toBeInTheDocument();
  });

  it("renders shortcut labels and key bindings", () => {
    renderWithShortcutsContext(true);
    expect(
      screen.getByText("Show keyboard shortcuts"),
    ).toBeInTheDocument();
    expect(screen.getByText("?")).toBeInTheDocument();
    expect(
      screen.getByText("Open command palette"),
    ).toBeInTheDocument();
    expect(screen.getByText("Ctrl+K")).toBeInTheDocument();
    expect(screen.getByText("Submit form")).toBeInTheDocument();
    expect(screen.getByText("Ctrl+Enter")).toBeInTheDocument();
  });

  it("does not render dialog content when closed", () => {
    renderWithShortcutsContext(false);
    expect(
      screen.queryByText("Keyboard Shortcuts"),
    ).not.toBeInTheDocument();
  });
});
