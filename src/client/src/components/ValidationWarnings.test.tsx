import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { ValidationWarnings } from "./ValidationWarnings";

describe("ValidationWarnings", () => {
  it("renders nothing when warnings array is empty", () => {
    const { container } = render(<ValidationWarnings warnings={[]} />);
    expect(container.firstChild).toBeNull();
  });

  it("renders a warning alert for severity 1", () => {
    render(
      <ValidationWarnings
        warnings={[
          {
            property: "TaxAmount",
            message: "Tax rate exceeds 25%",
            severity: 1,
          },
        ]}
      />,
    );

    expect(screen.getByText("Warning: TaxAmount")).toBeInTheDocument();
    expect(screen.getByText("Tax rate exceeds 25%")).toBeInTheDocument();
  });

  it("renders an info alert for severity 0", () => {
    render(
      <ValidationWarnings
        warnings={[
          {
            property: "AdjustmentTotal",
            message: "Large adjustment detected",
            severity: 0,
          },
        ]}
      />,
    );

    expect(
      screen.getByText("Info: AdjustmentTotal"),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Large adjustment detected"),
    ).toBeInTheDocument();
  });

  it("renders an info alert when severity is undefined", () => {
    render(
      <ValidationWarnings
        warnings={[
          { property: "Field", message: "Some info" },
        ]}
      />,
    );

    expect(screen.getByText("Info: Field")).toBeInTheDocument();
  });

  it("renders multiple warnings", () => {
    render(
      <ValidationWarnings
        warnings={[
          {
            property: "TaxAmount",
            message: "High tax",
            severity: 1,
          },
          {
            property: "AdjustmentTotal",
            message: "High adjustment",
            severity: 1,
          },
        ]}
      />,
    );

    expect(screen.getByText("Warning: TaxAmount")).toBeInTheDocument();
    expect(
      screen.getByText("Warning: AdjustmentTotal"),
    ).toBeInTheDocument();
    expect(screen.getByText("High tax")).toBeInTheDocument();
    expect(screen.getByText("High adjustment")).toBeInTheDocument();
  });
});
