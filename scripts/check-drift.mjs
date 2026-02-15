#!/usr/bin/env node

/**
 * Semantic drift detection: compares the hand-authored spec (openapi/spec.yaml)
 * against the build-time generated spec (openapi/generated/API.json).
 *
 * Performs deep structural comparison including:
 * - Path + method pairs (operations)
 * - Schema names and their property structures
 * - Property types, formats, and nullability
 * - $ref targets in request/response bodies
 * - Required field lists
 * - Response status codes per operation
 * - Path parameter counts per operation
 *
 * Intentionally ignores cosmetic differences:
 * - Path casing (ASP.NET lowercases routes)
 * - Type array ordering (["string", "null"] vs ["null", "string"])
 * - Extra content types in generated output (text/json, application/*+json)
 * - Extra "string" type + pattern on numeric fields (ASP.NET serialization hint)
 * - Description/summary text differences
 * - oneOf ordering for nullable $ref types
 * - Value-type required fields: ASP.NET never marks boolean/number/integer
 *   properties as required because C# value types always have defaults
 * - Missing response body schemas in generated output (e.g., health endpoint)
 *
 * Exits non-zero on any semantic mismatch.
 */

import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import yaml from "js-yaml";

const ROOT = resolve(import.meta.dirname, "..");
const SPEC_PATH = resolve(ROOT, "openapi/spec.yaml");
const GENERATED_PATH = resolve(ROOT, "openapi/generated/API.json");

/** @type {string[]} */
const errors = [];

function error(msg) {
  errors.push(msg);
}

function loadSpec() {
  try {
    const content = readFileSync(SPEC_PATH, "utf8");
    return yaml.load(content);
  } catch (err) {
    console.error(`Failed to load spec: ${SPEC_PATH}`);
    console.error(err.message);
    process.exit(1);
  }
}

function loadGenerated() {
  try {
    const content = readFileSync(GENERATED_PATH, "utf8");
    return JSON.parse(content);
  } catch (err) {
    console.error(`Failed to load generated spec: ${GENERATED_PATH}`);
    console.error(err.message);
    process.exit(1);
  }
}

/**
 * Normalize path: lowercase and replace {paramName} with {_} so that
 * paths like /api/Items/{id} and /api/items/{receiptId} are treated
 * as equivalent.
 */
function normalizePath(path) {
  return path.toLowerCase().replace(/\{[^}]+\}/g, "{_}");
}

/**
 * Build a map of normalized-path -> original-path for lookup.
 */
function buildPathMap(paths) {
  const map = new Map();
  for (const path of Object.keys(paths)) {
    map.set(normalizePath(path), path);
  }
  return map;
}

const HTTP_METHODS = [
  "get",
  "post",
  "put",
  "delete",
  "patch",
  "options",
  "head",
];

/**
 * Extract operations as a set of "METHOD /normalized-path" strings.
 */
function extractOperations(doc) {
  const ops = new Set();
  const paths = doc.paths || {};
  for (const [path, methods] of Object.entries(paths)) {
    for (const method of Object.keys(methods)) {
      if (HTTP_METHODS.includes(method)) {
        ops.add(`${method.toUpperCase()} ${normalizePath(path)}`);
      }
    }
  }
  return ops;
}

/**
 * Normalize a type value for comparison. Handles:
 * - Array types: sort and filter out extra "string" on numeric fields
 * - Single type strings
 */
function normalizeType(typeValue) {
  if (typeValue === undefined || typeValue === null) {
    return null;
  }

  if (Array.isArray(typeValue)) {
    // Filter out the extra "string" type that ASP.NET adds to numeric fields
    // (it adds "string" alongside "number"/"integer" for serialization flexibility)
    let types = [...typeValue];
    const hasNumeric = types.some((t) => t === "number" || t === "integer");
    if (hasNumeric && types.includes("string")) {
      types = types.filter((t) => t !== "string");
    }
    return types.sort().join("|");
  }

  return String(typeValue);
}

/**
 * Extract the $ref target from a schema, handling both direct $ref and oneOf patterns.
 * Returns the schema name (e.g., "AccountVM") or null.
 */
function extractRef(schema) {
  if (!schema) return null;
  if (schema.$ref) {
    return schema.$ref.replace("#/components/schemas/", "");
  }
  if (schema.oneOf) {
    for (const item of schema.oneOf) {
      if (item.$ref) {
        return item.$ref.replace("#/components/schemas/", "");
      }
    }
  }
  return null;
}

/**
 * Check if a schema represents a nullable type (via oneOf with null or type array with null).
 */
function isNullable(schema) {
  if (!schema) return false;
  if (Array.isArray(schema.type) && schema.type.includes("null")) return true;
  if (schema.oneOf) {
    return schema.oneOf.some(
      (item) => item.type === "null" || item.type === null,
    );
  }
  return false;
}

/**
 * Get the items schema ref from an array-typed schema.
 */
function getArrayItemsRef(schema) {
  if (!schema || !schema.items) return null;
  return extractRef(schema.items);
}

/**
 * Compare two property schemas for semantic equivalence.
 */
function comparePropertySchemas(propName, schemaName, specProp, genProp) {
  const prefix = `Schema "${schemaName}" property "${propName}"`;

  // Compare types
  const specType = normalizeType(specProp.type);
  const genType = normalizeType(genProp.type);
  if (specType !== genType) {
    error(
      `${prefix}: type mismatch — spec="${specType}", generated="${genType}"`,
    );
  }

  // Compare formats
  const specFormat = specProp.format || null;
  const genFormat = genProp.format || null;
  if (specFormat !== genFormat) {
    error(
      `${prefix}: format mismatch — spec="${specFormat}", generated="${genFormat}"`,
    );
  }

  // Compare $ref targets
  const specRef = extractRef(specProp);
  const genRef = extractRef(genProp);
  if (specRef !== genRef) {
    error(
      `${prefix}: $ref mismatch — spec="${specRef}", generated="${genRef}"`,
    );
  }

  // Compare nullability
  const specNullable = isNullable(specProp);
  const genNullable = isNullable(genProp);
  if (specNullable !== genNullable) {
    error(
      `${prefix}: nullability mismatch — spec=${specNullable}, generated=${genNullable}`,
    );
  }

  // Compare array items $ref
  const specItemsRef = getArrayItemsRef(specProp);
  const genItemsRef = getArrayItemsRef(genProp);
  if (specItemsRef !== genItemsRef) {
    error(
      `${prefix}: array items $ref mismatch — spec="${specItemsRef}", generated="${genItemsRef}"`,
    );
  }
}

/**
 * Compare schema definitions (components/schemas) between spec and generated.
 */
function compareSchemas(specDoc, generatedDoc) {
  const specSchemas = specDoc.components?.schemas || {};
  const genSchemas = generatedDoc.components?.schemas || {};

  const specNames = new Set(Object.keys(specSchemas));
  const genNames = new Set(Object.keys(genSchemas));

  // Check for missing/extra schemas
  for (const name of specNames) {
    if (!genNames.has(name)) {
      error(`Schema "${name}" exists in spec but NOT in generated output`);
    }
  }
  for (const name of genNames) {
    if (!specNames.has(name)) {
      error(`Schema "${name}" exists in generated output but NOT in spec`);
    }
  }

  // Deep-compare schemas that exist in both
  for (const name of specNames) {
    if (!genNames.has(name)) continue;

    const specSchema = specSchemas[name];
    const genSchema = genSchemas[name];

    // Compare schema type
    if (specSchema.type !== genSchema.type) {
      error(
        `Schema "${name}": type mismatch — spec="${specSchema.type}", generated="${genSchema.type}"`,
      );
    }

    const specProps = specSchema.properties || {};
    const genProps = genSchema.properties || {};
    const specPropNames = new Set(Object.keys(specProps));
    const genPropNames = new Set(Object.keys(genProps));

    // Check for missing/extra properties
    for (const prop of specPropNames) {
      if (!genPropNames.has(prop)) {
        error(
          `Schema "${name}": property "${prop}" exists in spec but NOT in generated`,
        );
      }
    }
    for (const prop of genPropNames) {
      if (!specPropNames.has(prop)) {
        error(
          `Schema "${name}": property "${prop}" exists in generated but NOT in spec`,
        );
      }
    }

    // Deep-compare properties that exist in both
    for (const prop of specPropNames) {
      if (!genPropNames.has(prop)) continue;
      comparePropertySchemas(prop, name, specProps[prop], genProps[prop]);
    }

    // Compare required fields.
    // Skip value-type properties (boolean, number, integer) when checking
    // spec-required vs generated-required — ASP.NET never marks C# value types
    // as required because they always have a default value.
    const specRequired = new Set(specSchema.required || []);
    const genRequired = new Set(genSchema.required || []);
    for (const field of specRequired) {
      if (!genRequired.has(field)) {
        const propSchema = specProps[field];
        const propType = propSchema
          ? Array.isArray(propSchema.type)
            ? propSchema.type.find((t) => t !== "null")
            : propSchema.type
          : null;
        const isValueType =
          propType === "boolean" ||
          propType === "number" ||
          propType === "integer";
        if (!isValueType) {
          error(
            `Schema "${name}": field "${field}" is required in spec but NOT in generated`,
          );
        }
      }
    }
    for (const field of genRequired) {
      if (!specRequired.has(field)) {
        error(
          `Schema "${name}": field "${field}" is required in generated but NOT in spec`,
        );
      }
    }
  }
}

/**
 * Extract the primary schema from a response/request content object.
 * Looks at application/json content type only.
 */
function extractContentSchema(content) {
  if (!content) return null;
  const jsonContent = content["application/json"];
  if (!jsonContent || !jsonContent.schema) return null;
  return jsonContent.schema;
}

/**
 * Compare request/response schemas for a given operation.
 */
function compareOperationSchemas(specOp, genOp, operationLabel) {
  // Compare response schemas for success responses
  for (const statusCode of ["200", "201", "204"]) {
    const specResponse = specOp.responses?.[statusCode];
    const genResponse = genOp.responses?.[statusCode];

    if (specResponse && genResponse) {
      const specSchema = extractContentSchema(specResponse.content);
      const genSchema = extractContentSchema(genResponse.content);

      if (specSchema && genSchema) {
        const specRef = extractRef(specSchema);
        const genRef = extractRef(genSchema);
        if (specRef !== genRef) {
          error(
            `${operationLabel} response ${statusCode}: $ref mismatch — spec="${specRef}", generated="${genRef}"`,
          );
        }

        const specItemsRef = getArrayItemsRef(specSchema);
        const genItemsRef = getArrayItemsRef(genSchema);
        if (specItemsRef !== genItemsRef) {
          error(
            `${operationLabel} response ${statusCode}: array items $ref mismatch — spec="${specItemsRef}", generated="${genItemsRef}"`,
          );
        }

        const specType = normalizeType(specSchema.type);
        const genType = normalizeType(genSchema.type);
        if (specType !== genType) {
          error(
            `${operationLabel} response ${statusCode}: type mismatch — spec="${specType}", generated="${genType}"`,
          );
        }
      } else if (!specSchema && genSchema) {
        error(
          `${operationLabel} response ${statusCode}: generated has schema but spec does not`,
        );
      }
    }
  }

  // Compare request body schema
  const specReqSchema = extractContentSchema(specOp.requestBody?.content);
  const genReqSchema = extractContentSchema(genOp.requestBody?.content);

  if (specReqSchema && genReqSchema) {
    const specRef = extractRef(specReqSchema);
    const genRef = extractRef(genReqSchema);
    if (specRef !== genRef) {
      error(
        `${operationLabel} requestBody: $ref mismatch — spec="${specRef}", generated="${genRef}"`,
      );
    }

    const specItemsRef = getArrayItemsRef(specReqSchema);
    const genItemsRef = getArrayItemsRef(genReqSchema);
    if (specItemsRef !== genItemsRef) {
      error(
        `${operationLabel} requestBody: array items $ref mismatch — spec="${specItemsRef}", generated="${genItemsRef}"`,
      );
    }

    const specType = normalizeType(specReqSchema.type);
    const genType = normalizeType(genReqSchema.type);
    if (specType !== genType) {
      error(
        `${operationLabel} requestBody: type mismatch — spec="${specType}", generated="${genType}"`,
      );
    }
  } else if (specReqSchema && !genReqSchema) {
    error(
      `${operationLabel} requestBody: spec has schema but generated does not`,
    );
  } else if (!specReqSchema && genReqSchema) {
    error(
      `${operationLabel} requestBody: generated has schema but spec does not`,
    );
  }

  // Compare path parameter counts
  const specParams = (specOp.parameters || []).filter((p) => p.in === "path");
  const genParams = (genOp.parameters || []).filter((p) => p.in === "path");

  if (specParams.length !== genParams.length) {
    error(
      `${operationLabel}: path parameter count mismatch — spec=${specParams.length}, generated=${genParams.length}`,
    );
  }

  // Compare response status codes
  const specStatuses = new Set(Object.keys(specOp.responses || {}));
  const genStatuses = new Set(Object.keys(genOp.responses || {}));

  for (const status of specStatuses) {
    if (!genStatuses.has(status)) {
      error(
        `${operationLabel}: response status "${status}" in spec but NOT in generated`,
      );
    }
  }
  for (const status of genStatuses) {
    if (!specStatuses.has(status)) {
      error(
        `${operationLabel}: response status "${status}" in generated but NOT in spec`,
      );
    }
  }
}

/**
 * Compare operations between spec and generated, including their schemas.
 */
function compareOperations(specDoc, generatedDoc) {
  const specPaths = specDoc.paths || {};
  const genPaths = generatedDoc.paths || {};

  const specPathMap = buildPathMap(specPaths);
  const genPathMap = buildPathMap(genPaths);

  for (const [normalizedPath, specOriginalPath] of specPathMap) {
    const genOriginalPath = genPathMap.get(normalizedPath);
    if (!genOriginalPath) continue;

    const specMethods = specPaths[specOriginalPath];
    const genMethods = genPaths[genOriginalPath];

    for (const method of HTTP_METHODS) {
      if (specMethods[method] && genMethods[method]) {
        const label = `${method.toUpperCase()} ${normalizedPath}`;
        compareOperationSchemas(specMethods[method], genMethods[method], label);
      }
    }
  }
}

function main() {
  const spec = loadSpec();
  const generated = loadGenerated();

  // 1. Compare operations (path + method pairs)
  const specOps = extractOperations(spec);
  const generatedOps = extractOperations(generated);

  const missingFromGenerated = [...specOps].filter(
    (op) => !generatedOps.has(op),
  );
  const extraInGenerated = [...generatedOps].filter((op) => !specOps.has(op));

  for (const op of missingFromGenerated.sort()) {
    error(`Operation in spec but NOT in generated: ${op}`);
  }
  for (const op of extraInGenerated.sort()) {
    error(`Operation in generated but NOT in spec: ${op}`);
  }

  // 2. Deep-compare schema definitions (properties, types, formats, $refs)
  compareSchemas(spec, generated);

  // 3. Compare operation request/response schemas and status codes
  compareOperations(spec, generated);

  if (errors.length > 0) {
    console.error("Drift detected between spec and generated output:\n");
    for (const err of errors) {
      console.error(`  - ${err}`);
    }
    console.error(
      `\n${errors.length} difference(s) found. Update openapi/spec.yaml to match.`,
    );
    process.exit(1);
  }

  console.log(
    "No drift detected between spec and generated output (semantic comparison).",
  );
}

main();
