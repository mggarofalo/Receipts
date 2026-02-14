#!/usr/bin/env node

/**
 * Drift detection: compares the hand-authored spec (openapi/spec.yaml)
 * against the build-time generated spec (openapi/generated/API.json).
 *
 * Checks structural alignment only: paths, methods, and schema names.
 * Exits non-zero on mismatch.
 */

import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import yaml from "js-yaml";

const ROOT = resolve(import.meta.dirname, "..");
const SPEC_PATH = resolve(ROOT, "openapi/spec.yaml");
const GENERATED_PATH = resolve(ROOT, "openapi/generated/API.json");

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
 * Normalize path parameters: replace {paramName} with {_} so that
 * paths like /api/Items/{id} and /api/Items/{receiptId} are treated
 * as equivalent (OpenAPI considers them the same route).
 */
function normalizePath(path) {
  return path.replace(/\{[^}]+\}/g, "{_}");
}

/**
 * Extract a sorted set of "METHOD /normalized-path" strings from an OpenAPI document.
 */
function extractOperations(doc) {
  const ops = new Set();
  const paths = doc.paths || {};
  for (const [path, methods] of Object.entries(paths)) {
    for (const method of Object.keys(methods)) {
      if (
        ["get", "post", "put", "delete", "patch", "options", "head"].includes(
          method,
        )
      ) {
        ops.add(`${method.toUpperCase()} ${normalizePath(path)}`);
      }
    }
  }
  return ops;
}

/**
 * Extract sorted schema names from an OpenAPI document.
 */
function extractSchemaNames(doc) {
  const schemas = doc.components?.schemas || {};
  return new Set(Object.keys(schemas));
}

function main() {
  const spec = loadSpec();
  const generated = loadGenerated();

  let driftDetected = false;

  // Compare operations (path + method pairs)
  const specOps = extractOperations(spec);
  const generatedOps = extractOperations(generated);

  const missingFromGenerated = [...specOps].filter((op) =>
    !generatedOps.has(op)
  );
  const extraInGenerated = [...generatedOps].filter((op) => !specOps.has(op));

  if (missingFromGenerated.length > 0) {
    console.error("Operations in spec but NOT in generated output:");
    for (const op of missingFromGenerated.sort()) {
      console.error(`  - ${op}`);
    }
    driftDetected = true;
  }

  if (extraInGenerated.length > 0) {
    console.error("Operations in generated output but NOT in spec:");
    for (const op of extraInGenerated.sort()) {
      console.error(`  + ${op}`);
    }
    driftDetected = true;
  }

  // Compare schema names
  const specSchemas = extractSchemaNames(spec);
  const generatedSchemas = extractSchemaNames(generated);

  const missingSchemas = [...specSchemas].filter(
    (s) => !generatedSchemas.has(s),
  );
  const extraSchemas = [...generatedSchemas].filter(
    (s) => !specSchemas.has(s),
  );

  if (missingSchemas.length > 0) {
    console.error("Schemas in spec but NOT in generated output:");
    for (const s of missingSchemas.sort()) {
      console.error(`  - ${s}`);
    }
    driftDetected = true;
  }

  if (extraSchemas.length > 0) {
    console.error("Schemas in generated output but NOT in spec:");
    for (const s of extraSchemas.sort()) {
      console.error(`  + ${s}`);
    }
    driftDetected = true;
  }

  if (driftDetected) {
    console.error(
      "\nDrift detected between spec and generated output. Update openapi/spec.yaml to match.",
    );
    process.exit(1);
  }

  console.log("No drift detected between spec and generated output.");
}

main();
