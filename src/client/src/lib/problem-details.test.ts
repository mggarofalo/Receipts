import { describe, it, expect } from "vitest";
import {
  parseProblemDetails,
  extractFieldErrors,
  type ProblemDetailsError,
} from "./problem-details";

describe("parseProblemDetails", () => {
  it("returns null for null input", () => {
    expect(parseProblemDetails(null)).toBeNull();
  });

  it("returns null for undefined input", () => {
    expect(parseProblemDetails(undefined)).toBeNull();
  });

  it("returns null for non-object input", () => {
    expect(parseProblemDetails("error")).toBeNull();
    expect(parseProblemDetails(42)).toBeNull();
  });

  it("parses ProblemDetails with status", () => {
    const error = {
      type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      title: "One or more validation errors occurred.",
      status: 400,
      errors: { Amount: ["Amount must be non-zero"] },
    };

    const result = parseProblemDetails(error);
    expect(result).not.toBeNull();
    expect(result!.status).toBe(400);
    expect(result!.title).toBe(
      "One or more validation errors occurred.",
    );
  });

  it("parses ProblemDetails with title only", () => {
    const error = { title: "Validation failed" };
    const result = parseProblemDetails(error);
    expect(result).not.toBeNull();
    expect(result!.title).toBe("Validation failed");
  });

  it("returns null for object without status or title", () => {
    const error = { message: "something went wrong" };
    expect(parseProblemDetails(error)).toBeNull();
  });
});

describe("extractFieldErrors", () => {
  it("returns empty object when no errors", () => {
    const problem: ProblemDetailsError = { status: 400, title: "Error" };
    expect(extractFieldErrors(problem)).toEqual({});
  });

  it("extracts first error per field", () => {
    const problem: ProblemDetailsError = {
      status: 400,
      errors: {
        Amount: ["Must be non-zero", "Must be valid"],
        Type: ["Required"],
      },
    };

    const result = extractFieldErrors(problem);
    expect(result).toEqual({
      amount: "Must be non-zero",
      type: "Required",
    });
  });

  it("normalizes field names to camelCase", () => {
    const problem: ProblemDetailsError = {
      status: 400,
      errors: {
        Description: ["Required when type is other"],
      },
    };

    const result = extractFieldErrors(problem);
    expect(result).toEqual({
      description: "Required when type is other",
    });
  });

  it("skips fields with empty error arrays", () => {
    const problem: ProblemDetailsError = {
      status: 400,
      errors: {
        Amount: [],
        Type: ["Required"],
      },
    };

    const result = extractFieldErrors(problem);
    expect(result).toEqual({ type: "Required" });
  });
});
