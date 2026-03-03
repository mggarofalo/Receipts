import { vi } from "vitest";

const mockClient = {
  GET: vi.fn(),
  POST: vi.fn(),
  PUT: vi.fn(),
  DELETE: vi.fn(),
  use: vi.fn(),
};

export function mockApiSuccess<T>(data: T) {
  return { data, error: undefined };
}

export function mockApiError(error: unknown = { message: "Request failed" }) {
  return { data: undefined, error };
}

export function resetMockClient() {
  mockClient.GET.mockReset();
  mockClient.POST.mockReset();
  mockClient.PUT.mockReset();
  mockClient.DELETE.mockReset();
}

export default mockClient;
