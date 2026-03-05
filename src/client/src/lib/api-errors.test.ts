import {
  isTimeoutError,
  isNetworkError,
  getConnectionErrorMessage,
} from "./api-errors";

describe("isTimeoutError", () => {
  it("returns true for a DOMException with name TimeoutError", () => {
    const error = new DOMException("signal timed out", "TimeoutError");
    expect(isTimeoutError(error)).toBe(true);
  });

  it("returns false for a DOMException with a different name", () => {
    const error = new DOMException("aborted", "AbortError");
    expect(isTimeoutError(error)).toBe(false);
  });

  it("returns false for a plain Error", () => {
    expect(isTimeoutError(new Error("timeout"))).toBe(false);
  });

  it("returns false for non-error values", () => {
    expect(isTimeoutError(null)).toBe(false);
    expect(isTimeoutError(undefined)).toBe(false);
    expect(isTimeoutError("TimeoutError")).toBe(false);
  });
});

describe("isNetworkError", () => {
  it("returns true for a TypeError with 'Failed to fetch' message", () => {
    const error = new TypeError("Failed to fetch");
    expect(isNetworkError(error)).toBe(true);
  });

  it("returns false for a TypeError with a different message", () => {
    const error = new TypeError("something else");
    expect(isNetworkError(error)).toBe(false);
  });

  it("returns false for a plain Error with the same message", () => {
    expect(isNetworkError(new Error("Failed to fetch"))).toBe(false);
  });

  it("returns false for non-error values", () => {
    expect(isNetworkError(null)).toBe(false);
    expect(isNetworkError(undefined)).toBe(false);
  });
});

describe("getConnectionErrorMessage", () => {
  it("returns timeout message for TimeoutError", () => {
    const error = new DOMException("signal timed out", "TimeoutError");
    expect(getConnectionErrorMessage(error)).toBe(
      "The server is not responding. Please try again later.",
    );
  });

  it("returns network message for Failed to fetch", () => {
    const error = new TypeError("Failed to fetch");
    expect(getConnectionErrorMessage(error)).toBe(
      "Unable to connect to the server. Please check your connection and try again.",
    );
  });

  it("returns null for a generic error", () => {
    expect(getConnectionErrorMessage(new Error("fail"))).toBeNull();
  });

  it("returns null for non-error values", () => {
    expect(getConnectionErrorMessage(null)).toBeNull();
    expect(getConnectionErrorMessage("string error")).toBeNull();
  });
});
