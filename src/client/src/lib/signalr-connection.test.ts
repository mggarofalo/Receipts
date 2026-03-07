import { describe, it, expect, beforeEach } from "vitest";
import { getConnectionId, setConnectionId } from "./signalr-connection";

describe("signalr-connection", () => {
  beforeEach(() => {
    setConnectionId(null);
  });

  it("returns null by default", () => {
    expect(getConnectionId()).toBeNull();
  });

  it("stores and retrieves a connection ID", () => {
    setConnectionId("conn-123");
    expect(getConnectionId()).toBe("conn-123");
  });

  it("clears the connection ID when set to null", () => {
    setConnectionId("conn-123");
    setConnectionId(null);
    expect(getConnectionId()).toBeNull();
  });
});
