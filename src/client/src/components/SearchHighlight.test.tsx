import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { SearchHighlight } from "./SearchHighlight";

describe("SearchHighlight", () => {
  it("renders plain text when no indices are provided", () => {
    render(<SearchHighlight text="Hello World" />);
    expect(screen.getByText("Hello World")).toBeInTheDocument();
  });

  it("renders plain text when indices array is empty", () => {
    render(<SearchHighlight text="Hello World" indices={[]} />);
    expect(screen.getByText("Hello World")).toBeInTheDocument();
  });

  it("highlights matching ranges with mark elements", () => {
    const { container } = render(
      <SearchHighlight text="Hello World" indices={[[0, 4]]} />,
    );
    const marks = container.querySelectorAll("mark");
    expect(marks).toHaveLength(1);
    expect(marks[0]).toHaveTextContent("Hello");
  });

  it("renders non-highlighted text around the match", () => {
    const { container } = render(
      <SearchHighlight text="Hello World" indices={[[0, 4]]} />,
    );
    const marks = container.querySelectorAll("mark");
    expect(marks).toHaveLength(1);
    expect(marks[0]).toHaveTextContent("Hello");
    // The remaining " World" is a text node sibling
    expect(container.textContent).toBe("Hello World");
  });

  it("highlights multiple non-overlapping ranges", () => {
    const { container } = render(
      <SearchHighlight
        text="Hello World Test"
        indices={[
          [0, 4],
          [6, 10],
        ]}
      />,
    );
    const marks = container.querySelectorAll("mark");
    expect(marks).toHaveLength(2);
    expect(marks[0]).toHaveTextContent("Hello");
    expect(marks[1]).toHaveTextContent("World");
  });

  it("merges overlapping ranges", () => {
    const { container } = render(
      <SearchHighlight
        text="Hello World"
        indices={[
          [0, 3],
          [2, 6],
        ]}
      />,
    );
    const marks = container.querySelectorAll("mark");
    expect(marks).toHaveLength(1);
    expect(marks[0]).toHaveTextContent("Hello W");
  });

  it("applies custom highlight class name", () => {
    const { container } = render(
      <SearchHighlight
        text="Hello World"
        indices={[[0, 4]]}
        highlightClassName="custom-highlight"
      />,
    );
    const mark = container.querySelector("mark");
    expect(mark).toHaveClass("custom-highlight");
  });
});
