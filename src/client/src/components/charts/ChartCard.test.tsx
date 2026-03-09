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
});
