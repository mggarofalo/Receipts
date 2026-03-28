import { render, screen } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router";
import ReceiptDetailRedirect from "./ReceiptDetailRedirect";

function renderWithRoutes(initialRoute: string) {
  return render(
    <MemoryRouter initialEntries={[initialRoute]}>
      <Routes>
        <Route path="/receipt-detail" element={<ReceiptDetailRedirect />} />
        <Route path="/receipts/:id" element={<div data-testid="receipt-detail-page">Receipt Detail</div>} />
        <Route path="/receipts" element={<div data-testid="receipts-page">Receipts</div>} />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ReceiptDetailRedirect", () => {
  it("redirects to /receipts/:id when id query param is present", () => {
    renderWithRoutes("/receipt-detail?id=some-uuid");
    expect(screen.getByTestId("receipt-detail-page")).toBeInTheDocument();
  });

  it("redirects to /receipts when no id query param", () => {
    renderWithRoutes("/receipt-detail");
    expect(screen.getByTestId("receipts-page")).toBeInTheDocument();
  });

  it("redirects to /receipts when id query param is empty", () => {
    renderWithRoutes("/receipt-detail?id=");
    expect(screen.getByTestId("receipts-page")).toBeInTheDocument();
  });
});
