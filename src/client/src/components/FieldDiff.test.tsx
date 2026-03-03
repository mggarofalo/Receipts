import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { FieldDiff } from "./FieldDiff";

describe("FieldDiff", () => {
  it("renders the field name", () => {
    render(
      <FieldDiff fieldName="Amount" oldValue="100" newValue="200" />,
    );
    expect(screen.getByText("Amount")).toBeInTheDocument();
  });

  it("renders a create diff when oldValue is null", () => {
    render(
      <FieldDiff fieldName="Name" oldValue={null} newValue="Alice" />,
    );
    expect(screen.getByText("Alice")).toBeInTheDocument();
  });

  it("renders a delete diff when newValue is null", () => {
    render(
      <FieldDiff fieldName="Name" oldValue="Alice" newValue={null} />,
    );
    expect(screen.getByText("Alice")).toBeInTheDocument();
  });

  it("shows (empty) for create when newValue is empty string", () => {
    render(
      <FieldDiff fieldName="Notes" oldValue={null} newValue="" />,
    );
    expect(screen.getByText("(empty)")).toBeInTheDocument();
  });

  it("shows (empty) for delete when oldValue is empty string", () => {
    render(
      <FieldDiff fieldName="Notes" oldValue="" newValue={null} />,
    );
    expect(screen.getByText("(empty)")).toBeInTheDocument();
  });

  it("renders character-level diff for modified values", () => {
    const { container } = render(
      <FieldDiff fieldName="Description" oldValue="Hello" newValue="Help" />,
    );
    // The diff-match-patch library produces character-level diffs
    // There should be at least one span inside the font-mono element
    const diffContainer = container.querySelector(".font-mono");
    expect(diffContainer).toBeInTheDocument();
    expect(diffContainer!.textContent).toContain("Hel");
  });
});
