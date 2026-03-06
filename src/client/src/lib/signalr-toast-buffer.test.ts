import { describe, it, expect, vi, beforeEach } from "vitest";

vi.mock("sonner", () => ({
  toast: { info: vi.fn() },
}));

import { toast } from "sonner";
import {
  bufferToast,
  _resetForTesting,
  _flushForTesting,
} from "./signalr-toast-buffer";

beforeEach(() => {
  vi.clearAllMocks();
  _resetForTesting();
  vi.useFakeTimers();
});

describe("signalr-toast-buffer", () => {
  it("single event flushes as singular message", () => {
    bufferToast("receipt", "created", 1);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "A receipt was created by another user",
    );
    expect(toast.info).toHaveBeenCalledTimes(1);
  });

  it("multiple events of same key accumulate count", () => {
    bufferToast("receipt", "created", 1);
    bufferToast("receipt", "created", 1);
    bufferToast("receipt", "created", 1);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "3 receipts were created by another user",
    );
    expect(toast.info).toHaveBeenCalledTimes(1);
  });

  it("different keys produce separate toasts", () => {
    bufferToast("receipt", "created", 1);
    bufferToast("account", "updated", 1);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "A receipt was created by another user",
    );
    expect(toast.info).toHaveBeenCalledWith(
      "A account was updated by another user",
    );
    expect(toast.info).toHaveBeenCalledTimes(2);
  });

  it('pluralizes "category" to "categories"', () => {
    bufferToast("category", "deleted", 1);
    bufferToast("category", "deleted", 1);
    bufferToast("category", "deleted", 1);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "3 categories were deleted by another user",
    );
  });

  it('pluralizes "receipt" to "receipts"', () => {
    bufferToast("receipt", "updated", 1);
    bufferToast("receipt", "updated", 1);
    bufferToast("receipt", "updated", 1);
    bufferToast("receipt", "updated", 1);
    bufferToast("receipt", "updated", 1);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "5 receipts were updated by another user",
    );
  });

  it("count parameter adds to existing count", () => {
    bufferToast("receipt", "deleted", 3);
    bufferToast("receipt", "deleted", 2);
    _flushForTesting();

    expect(toast.info).toHaveBeenCalledWith(
      "5 receipts were deleted by another user",
    );
    expect(toast.info).toHaveBeenCalledTimes(1);
  });

  it("_resetForTesting clears without flushing", () => {
    bufferToast("receipt", "created", 1);
    bufferToast("account", "updated", 1);
    _resetForTesting();
    _flushForTesting();

    expect(toast.info).not.toHaveBeenCalled();
  });

  it("flush with no buffered items produces no toasts", () => {
    _flushForTesting();

    expect(toast.info).not.toHaveBeenCalled();
  });
});
