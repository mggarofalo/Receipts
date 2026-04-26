import { describe, expect, it } from "vitest";
import { scanProposal } from "./scan";
import {
  mapProposalToInitialValues,
  mapProposalToConfidenceMap,
} from "@/pages/scan-receipt/proposalMappers";

/**
 * Regression coverage for RECEIPTS-632.
 *
 * The MSW scan fixture used to drift from the OpenAPI contract because it
 * was a bare object literal with no type annotation — TypeScript silently
 * accepted missing required fields, and integration tests crashed at runtime
 * with `TypeError: Cannot read properties of undefined`.
 *
 * These tests lock down the fixture's shape against the same proposal
 * mappers that production code uses, so any future drift surfaces here
 * (and in the type-check) instead of in a downstream test.
 */
describe("scanProposal fixture", () => {
  it("can be mapped to initial values without throwing", () => {
    expect(() => mapProposalToInitialValues(scanProposal)).not.toThrow();
  });

  it("can be mapped to a confidence map without throwing", () => {
    expect(() => mapProposalToConfidenceMap(scanProposal)).not.toThrow();
  });

  it("provides a non-empty payments array", () => {
    // The wizard renders payment chips; an empty array silently hides them
    // and would mask a real regression in upstream payment extraction.
    expect(scanProposal.payments.length).toBeGreaterThan(0);
    for (const payment of scanProposal.payments) {
      expect(payment).toHaveProperty("method");
      expect(payment).toHaveProperty("amount");
      expect(payment).toHaveProperty("lastFour");
      expect(payment).toHaveProperty("methodConfidence");
      expect(payment).toHaveProperty("amountConfidence");
      expect(payment).toHaveProperty("lastFourConfidence");
    }
  });

  it("provides taxCode and taxCodeConfidence for every item", () => {
    // Per-item taxCode is required by the OpenAPI contract; missing it
    // crashes mapProposalToInitialValues at runtime.
    expect(scanProposal.items.length).toBeGreaterThan(0);
    for (const item of scanProposal.items) {
      expect(item).toHaveProperty("taxCode");
      expect(item).toHaveProperty("taxCodeConfidence");
    }
  });

  it("populates every required top-level metadata field", () => {
    // These fields were added during VLM rollout and are required by the
    // OpenAPI contract; the fixture must keep them populated.
    expect(scanProposal).toHaveProperty("storeAddress");
    expect(scanProposal).toHaveProperty("storeAddressConfidence");
    expect(scanProposal).toHaveProperty("storePhone");
    expect(scanProposal).toHaveProperty("storePhoneConfidence");
    expect(scanProposal).toHaveProperty("receiptId");
    expect(scanProposal).toHaveProperty("receiptIdConfidence");
    expect(scanProposal).toHaveProperty("storeNumber");
    expect(scanProposal).toHaveProperty("storeNumberConfidence");
    expect(scanProposal).toHaveProperty("terminalId");
    expect(scanProposal).toHaveProperty("terminalIdConfidence");
  });

  it("exercises a representative mix of confidence levels", () => {
    // The fixture exists to drive UI badge rendering; keeping a mix of
    // high/medium/low ensures the wizard's confidence visuals stay covered.
    const allConfidences = new Set<string>();
    allConfidences.add(scanProposal.storeNameConfidence);
    allConfidences.add(scanProposal.storeAddressConfidence);
    allConfidences.add(scanProposal.storePhoneConfidence);
    allConfidences.add(scanProposal.dateConfidence);
    allConfidences.add(scanProposal.terminalIdConfidence);
    allConfidences.add(scanProposal.storeNumberConfidence);
    expect(allConfidences.size).toBeGreaterThan(1);
  });
});
