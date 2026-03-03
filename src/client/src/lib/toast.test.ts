import { showSuccess, showError, showApiError, showNetworkError } from "./toast";
import { toast } from "sonner";
import { clearTokens } from "@/lib/auth";

vi.mock("sonner", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

vi.mock("@/lib/auth", () => ({
  clearTokens: vi.fn(),
}));

const locationHrefSpy = vi.spyOn(window, "location", "get");

beforeEach(() => {
  vi.clearAllMocks();
  locationHrefSpy.mockReturnValue({
    ...window.location,
    href: "/",
  } as Location);
});

describe("showSuccess", () => {
  it("calls toast.success", () => {
    showSuccess("Done!");
    expect(toast.success).toHaveBeenCalledWith("Done!");
  });
});

describe("showError", () => {
  it("calls toast.error", () => {
    showError("Oops");
    expect(toast.error).toHaveBeenCalledWith("Oops");
  });
});

describe("showApiError", () => {
  it("handles 401 — clears tokens and redirects", () => {
    const hrefSetter = vi.fn();
    locationHrefSpy.mockReturnValue(
      Object.defineProperty({ ...window.location } as Location, "href", {
        set: hrefSetter,
        get: () => "/",
      }),
    );

    showApiError(401);
    expect(toast.error).toHaveBeenCalledWith(
      "Session expired. Please log in again.",
    );
    expect(clearTokens).toHaveBeenCalled();
  });

  it("handles 403", () => {
    showApiError(403);
    expect(toast.error).toHaveBeenCalledWith(
      "You do not have permission to perform this action.",
    );
  });

  it("handles 404 with default message", () => {
    showApiError(404);
    expect(toast.error).toHaveBeenCalledWith(
      "The requested resource was not found.",
    );
  });

  it("handles 404 with custom message", () => {
    showApiError(404, "Account not found");
    expect(toast.error).toHaveBeenCalledWith("Account not found");
  });

  it("handles 500 with default message", () => {
    showApiError(500);
    expect(toast.error).toHaveBeenCalledWith(
      "A server error occurred. Please try again.",
    );
  });

  it("handles unknown status with default message", () => {
    showApiError(422);
    expect(toast.error).toHaveBeenCalledWith("Request failed (422).");
  });

  it("handles unknown status with custom message", () => {
    showApiError(422, "Validation failed");
    expect(toast.error).toHaveBeenCalledWith("Validation failed");
  });
});

describe("showNetworkError", () => {
  it("shows network error toast", () => {
    showNetworkError();
    expect(toast.error).toHaveBeenCalledWith(
      "Network error. Please check your connection and try again.",
    );
  });
});
