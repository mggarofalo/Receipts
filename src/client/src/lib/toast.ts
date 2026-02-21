import { toast } from "sonner";
import { clearTokens } from "@/lib/auth";

export function showSuccess(message: string) {
  toast.success(message);
}

export function showError(message: string) {
  toast.error(message);
}

export function showApiError(status: number, message?: string) {
  switch (status) {
    case 401:
      toast.error("Session expired. Please log in again.");
      clearTokens();
      window.location.href = "/login";
      break;
    case 403:
      toast.error("You do not have permission to perform this action.");
      break;
    case 404:
      toast.error(message ?? "The requested resource was not found.");
      break;
    case 500:
      toast.error(message ?? "A server error occurred. Please try again.");
      break;
    default:
      toast.error(message ?? `Request failed (${status}).`);
  }
}

export function showNetworkError() {
  toast.error("Network error. Please check your connection and try again.");
}
