import { screen } from "@testing-library/react";
import { renderWithProviders } from "@/test/test-utils";
import { ChartCard } from "./ChartCard";

describe("ChartCard", () => {
  it("renders title and children", () => {
    renderWithProviders(
      <ChartCard title="Test Title">
        <div>Chart content</div>
      </ChartCard>,
    );
    expect(screen.getByText("Test Title")).toBeInTheDocument();
    expect(screen.getByText("Chart content")).toBeInTheDocument();
  });

  it("renders subtitle when provided", () => {
    renderWithProviders(
      <ChartCard title="Title" subtitle="Subtitle text">
        <div>Content</div>
      </ChartCard>,
    );
    expect(screen.getByText("Subtitle text")).toBeInTheDocument();
  });

  it("renders loading skeleton when loading", () => {
    renderWithProviders(
      <ChartCard title="Title" loading>
        <div>Content</div>
      </ChartCard>,
    );
    expect(screen.getByLabelText("Loading")).toBeInTheDocument();
    expect(screen.queryByText("Content")).not.toBeInTheDocument();
  });

  it("renders empty message when empty", () => {
    renderWithProviders(
      <ChartCard title="Title" empty>
        <div>Content</div>
      </ChartCard>,
    );
    expect(screen.getByText("No data available")).toBeInTheDocument();
    expect(screen.queryByText("Content")).not.toBeInTheDocument();
  });

  it("renders custom empty message", () => {
    renderWithProviders(
      <ChartCard title="Title" empty emptyMessage="Nothing here">
        <div>Content</div>
      </ChartCard>,
    );
    expect(screen.getByText("Nothing here")).toBeInTheDocument();
  });

  it("renders action slot", () => {
    renderWithProviders(
      <ChartCard title="Title" action={<button>Action</button>}>
        <div>Content</div>
      </ChartCard>,
    );
    expect(
      screen.getByRole("button", { name: "Action" }),
    ).toBeInTheDocument();
  });

  // Accessibility tests
  it("assigns an id to the CardTitle element", () => {
    renderWithProviders(
      <ChartCard title="Spending Overview">
        <div>Chart content</div>
      </ChartCard>,
    );
    // The title should have an id so chart children can reference it via aria-labelledby
    const titleEl = screen.getByText("Spending Overview");
    expect(titleEl).toHaveAttribute("id");
    expect(titleEl.getAttribute("id")).not.toBe("");
  });

  it("passes titleId to render-prop children", () => {
    let capturedId = "";
    renderWithProviders(
      <ChartCard title="My Chart">
        {(titleId) => {
          capturedId = titleId;
          return <div aria-labelledby={titleId}>chart</div>;
        }}
      </ChartCard>,
    );
    expect(capturedId).not.toBe("");
    const titleEl = screen.getByText("My Chart");
    expect(titleEl).toHaveAttribute("id", capturedId);
  });

  it("renders regular ReactNode children without render-prop pattern", () => {
    renderWithProviders(
      <ChartCard title="Title">
        <span data-testid="plain-child">plain</span>
      </ChartCard>,
    );
    expect(screen.getByTestId("plain-child")).toBeInTheDocument();
  });
});
