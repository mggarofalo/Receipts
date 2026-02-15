#!/usr/bin/env node

/**
 * Breaking change detection: compares the current openapi/spec.yaml against
 * a base branch version to detect backwards-incompatible API changes.
 *
 * Usage:
 *   node scripts/check-breaking.mjs <git-ref>
 *   node scripts/check-breaking.mjs origin/master
 *
 * The script reads the base spec via `git show <ref>:openapi/spec.yaml`
 * and compares it against the current working copy.
 *
 * Breaking changes detected:
 * - Removed endpoints (method + path)
 * - Removed schemas
 * - Removed properties from existing schemas
 * - Property type changes
 * - Property format changes
 * - New required fields in request schemas (Create*Request, Update*Request)
 * - Changed $ref targets
 * - Nullability narrowing (nullable → non-nullable)
 *
 * Exits non-zero if any breaking changes are found.
 */

import { readFileSync } from "node:fs";
import { execSync } from "node:child_process";
import { resolve } from "node:path";
import yaml from "js-yaml";

const ROOT = resolve(import.meta.dirname, "..");
const SPEC_PATH = resolve(ROOT, "openapi/spec.yaml");

/** @type {string[]} */
const breaking = [];

function flag(msg) {
  breaking.push(msg);
}

function usage() {
  console.error(
    "Usage: node scripts/check-breaking.mjs <git-ref>\n" +
      "Example: node scripts/check-breaking.mjs origin/master",
  );
  process.exit(2);
}

function loadBaseSpec(gitRef) {
  try {
    // Sanitize gitRef to prevent command injection - only allow safe characters
    if (!/^[\w\/\-\.]+$/.test(gitRef)) {
      throw new Error(`Invalid git reference: ${gitRef}`);
    }

    const content = execSync(`git show "${gitRef}:openapi/spec.yaml"`, {
      cwd: ROOT,
      encoding: "utf8",
      stdio: ["pipe", "pipe", "pipe"],
    });
    return yaml.load(content);
  } catch {
    console.error(
      `Could not read openapi/spec.yaml from ref "${gitRef}".\n` +
        "Make sure the ref exists and the file is present on that branch.",
    );
    process.exit(2);
  }
}

function loadCurrentSpec() {
  try {
    return yaml.load(readFileSync(SPEC_PATH, "utf8"));
  } catch (err) {
    console.error(`Failed to load current spec: ${SPEC_PATH}`);
    console.error(err.message);
    process.exit(1);
  }
}

/**
 * Normalize path for comparison: lowercase, replace param names with {_}.
 */
function normalizePath(path) {
  return path.toLowerCase().replace(/\{[^}]+\}/g, "{_}");
}

const HTTP_METHODS = ["get", "post", "put", "delete", "patch", "options", "head"];

/**
 * Extract operations as "METHOD /normalized-path" strings.
 */
function extractOperations(doc) {
  const ops = new Set();
  for (const [path, methods] of Object.entries(doc.paths || {})) {
    for (const method of Object.keys(methods)) {
      if (HTTP_METHODS.includes(method)) {
        ops.add(`${method.toUpperCase()} ${normalizePath(path)}`);
      }
    }
  }
  return ops;
}

/**
 * Normalize a type value for comparison.
 */
function normalizeType(typeValue) {
  if (typeValue === undefined || typeValue === null) return null;
  if (Array.isArray(typeValue)) return [...typeValue].sort().join("|");
  return String(typeValue);
}

/**
 * Extract $ref target name from a schema.
 */
function extractRef(schema) {
  if (!schema) return null;
  if (schema.$ref) return schema.$ref.replace("#/components/schemas/", "");
  if (schema.oneOf) {
    for (const item of schema.oneOf) {
      if (item.$ref) return item.$ref.replace("#/components/schemas/", "");
    }
  }
  return null;
}

/**
 * Check if a schema is nullable.
 */
function isNullable(schema) {
  if (!schema) return false;
  if (Array.isArray(schema.type) && schema.type.includes("null")) return true;
  if (schema.oneOf) {
    return schema.oneOf.some((item) => item.type === "null" || item.type === null);
  }
  return false;
}

/**
 * Whether a schema name is a request schema (sent by clients).
 */
function isRequestSchema(name) {
  return name.endsWith("Request");
}

/**
 * Compare schemas between base and current to detect breaking changes.
 */
function compareSchemas(baseDoc, currentDoc) {
  const baseSchemas = baseDoc.components?.schemas || {};
  const currentSchemas = currentDoc.components?.schemas || {};

  // Removed schemas
  for (const name of Object.keys(baseSchemas)) {
    if (!(name in currentSchemas)) {
      flag(`Schema removed: "${name}"`);
      continue;
    }

    const baseSch = baseSchemas[name];
    const currSch = currentSchemas[name];
    const baseProps = baseSch.properties || {};
    const currProps = currSch.properties || {};

    // Removed properties
    for (const prop of Object.keys(baseProps)) {
      if (!(prop in currProps)) {
        flag(`Schema "${name}": property "${prop}" removed`);
        continue;
      }

      const baseProp = baseProps[prop];
      const currProp = currProps[prop];

      // Type change
      const baseType = normalizeType(baseProp.type);
      const currType = normalizeType(currProp.type);
      if (baseType !== currType) {
        flag(
          `Schema "${name}" property "${prop}": type changed from "${baseType}" to "${currType}"`,
        );
      }

      // Format change
      const baseFmt = baseProp.format || null;
      const currFmt = currProp.format || null;
      if (baseFmt !== currFmt) {
        flag(
          `Schema "${name}" property "${prop}": format changed from "${baseFmt}" to "${currFmt}"`,
        );
      }

      // $ref change
      const baseRef = extractRef(baseProp);
      const currRef = extractRef(currProp);
      if (baseRef !== currRef) {
        flag(
          `Schema "${name}" property "${prop}": $ref changed from "${baseRef}" to "${currRef}"`,
        );
      }

      // Nullability narrowing (nullable → non-nullable is breaking)
      const baseNullable = isNullable(baseProp);
      const currNullable = isNullable(currProp);
      if (baseNullable && !currNullable) {
        flag(
          `Schema "${name}" property "${prop}": nullability narrowed (was nullable, now non-nullable)`,
        );
      }
    }

    // New required fields in request schemas (breaks existing clients)
    if (isRequestSchema(name)) {
      const baseRequired = new Set(baseSch.required || []);
      const currRequired = new Set(currSch.required || []);
      for (const field of currRequired) {
        if (!baseRequired.has(field)) {
          flag(
            `Schema "${name}": new required field "${field}" added (breaks existing clients)`,
          );
        }
      }
    }
  }
}

function main() {
  const gitRef = process.argv[2];
  if (!gitRef) {
    usage();
  }

  const baseSpec = loadBaseSpec(gitRef);
  const currentSpec = loadCurrentSpec();

  // 1. Removed endpoints
  const baseOps = extractOperations(baseSpec);
  const currentOps = extractOperations(currentSpec);

  for (const op of baseOps) {
    if (!currentOps.has(op)) {
      flag(`Endpoint removed: ${op}`);
    }
  }

  // 2. Schema-level breaking changes
  compareSchemas(baseSpec, currentSpec);

  if (breaking.length > 0) {
    console.error("Breaking API changes detected:\n");
    for (const b of breaking) {
      console.error(`  - ${b}`);
    }
    console.error(
      `\n${breaking.length} breaking change(s) found.` +
        "\nIf intentional, add the 'breaking-changes-allowed' label to the PR.",
    );
    process.exit(1);
  }

  console.log("No breaking API changes detected.");
}

main();
