import { renderHook, act } from "@testing-library/react";
import { useSubmitTimeout } from "./useSubmitTimeout";

describe("useSubmitTimeout", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("returns isWarning: false when not submitting", () => {
    const { result } = renderHook(() => useSubmitTimeout(false));
    expect(result.current.isWarning).toBe(false);
  });

  it("returns isWarning: false immediately when submitting starts", () => {
    const { result } = renderHook(() => useSubmitTimeout(true));
    expect(result.current.isWarning).toBe(false);
  });

  it("returns isWarning: true after 5 seconds of submitting", () => {
    const { result } = renderHook(() => useSubmitTimeout(true));

    act(() => {
      vi.advanceTimersByTime(5_000);
    });

    expect(result.current.isWarning).toBe(true);
  });

  it("resets isWarning to false when submitting ends", () => {
    const { result, rerender } = renderHook(
      ({ isSubmitting }) => useSubmitTimeout(isSubmitting),
      { initialProps: { isSubmitting: true } },
    );

    act(() => {
      vi.advanceTimersByTime(5_000);
    });
    expect(result.current.isWarning).toBe(true);

    rerender({ isSubmitting: false });
    expect(result.current.isWarning).toBe(false);
  });

  it("cleans up timer on unmount", () => {
    const { unmount } = renderHook(() => useSubmitTimeout(true));
    unmount();

    // Advancing timers after unmount should not cause errors
    act(() => {
      vi.advanceTimersByTime(5_000);
    });
  });
});
