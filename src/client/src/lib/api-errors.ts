export function isTimeoutError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "TimeoutError";
}

export function isNetworkError(error: unknown): boolean {
  return error instanceof TypeError && error.message === "Failed to fetch";
}

export function getConnectionErrorMessage(error: unknown): string | null {
  if (isTimeoutError(error)) {
    return "The server is not responding. Please try again later.";
  }
  if (isNetworkError(error)) {
    return "Unable to connect to the server. Please check your connection and try again.";
  }
  return null;
}
