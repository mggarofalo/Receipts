#!/usr/bin/env node

/**
 * Syncs Snyk SARIF findings to Linear issues with fingerprint-based deduplication
 * and a two-scan confirmation lifecycle for auto-closing resolved vulnerabilities.
 *
 * Usage: node scripts/create-linear-issues.mjs <sarif-file>
 *
 * Required env vars:
 *   LINEAR_API_KEY — Linear personal API key
 *
 * Auto-provided by GitHub Actions:
 *   GITHUB_SERVER_URL, GITHUB_REPOSITORY, GITHUB_RUN_ID
 */

import { readFileSync } from "node:fs";
import { LinearClient } from "@linear/sdk";

// --- Workspace constants (stable IDs, not secrets) ---
const TEAM_ID = "a4aff05d-41e6-45dc-b670-cdb485fef765";
const PROJECT_ID = "06199e4e-d3a8-4f98-9edf-e1e02efb6cee";

// Label IDs
const LABEL_SECURITY = "ee5f0431-4384-4c19-b83d-45ed2a34c9c4";
const LABEL_BACKEND = "f5657ea9-462c-42f3-bb3b-1885eb43a3e3";
const LABEL_INFRA = "3a0c50c8-5ad3-4f3a-9d42-4bce3f4bde9c";
const LABEL_SNYK = "ac0e5039-260b-42db-a3bd-8d23a9034895";
const LABEL_RESOLVED = "46afab2a-f987-40ab-a3f5-47809eaa7c70";

// State IDs
const STATE_BACKLOG = "601c0945-c05f-4602-ab32-9267a1dc0090";
const STATE_DONE = "5cd41ab3-b7b2-4788-b15e-2b40afa4f837";

// Fingerprint metadata marker regex
const FINGERPRINT_RE = /<!-- snyk:fingerprint:(.+?) -->/;

// --- SARIF parsing ---

/**
 * Maps SARIF level + priority score to Linear priority.
 * SARIF uses error/warning/note — NOT critical/high/medium.
 */
function mapPriority(level, priorityScore) {
  if (level === "error") return priorityScore >= 700 ? 1 : 2;
  if (level === "warning") return 3;
  return 4; // note, none, or unknown
}

/**
 * Parses a SARIF file and returns a Map<fingerprint, Finding>.
 * Skips results that lack a snyk/asset/finding/v1 fingerprint.
 */
function parseSarifResults(filePath) {
  const findings = new Map();

  let sarif;
  try {
    sarif = JSON.parse(readFileSync(filePath, "utf8"));
  } catch (err) {
    console.error(`Failed to read/parse SARIF file: ${err.message}`);
    return findings;
  }

  const runs = sarif.runs ?? [];
  for (const run of runs) {
    for (const result of run.results ?? []) {
      const props = result.properties ?? {};
      const findingMeta = props["snyk/asset/finding/v1"];
      const fingerprint = findingMeta?.fingerprint;

      if (!fingerprint) {
        console.warn(
          `Skipping result without fingerprint: ${result.ruleId ?? "unknown rule"}`,
        );
        continue;
      }

      const location = result.locations?.[0]?.physicalLocation;
      findings.set(fingerprint, {
        fingerprint,
        ruleId: result.ruleId ?? "unknown",
        level: result.level ?? "warning",
        priorityScore: props.priorityScore ?? 0,
        message: result.message?.text ?? "Security vulnerability detected",
        filePath: location?.artifactLocation?.uri ?? "unknown",
        startLine: location?.region?.startLine ?? 0,
      });
    }
  }

  return findings;
}

// --- Linear helpers ---

function truncate(str, max) {
  return str.length > max ? str.slice(0, max - 1) + "\u2026" : str;
}

function buildDescription(finding) {
  const runUrl = buildRunUrl();
  const lines = [
    `**Severity:** ${finding.level} (score: ${finding.priorityScore})`,
    `**File:** \`${finding.filePath}:${finding.startLine}\``,
    `**Rule:** ${finding.ruleId}`,
    "",
    finding.message,
  ];
  if (runUrl) {
    lines.push("", `**CI Run:** ${runUrl}`);
  }
  lines.push("", `<!-- snyk:fingerprint:${finding.fingerprint} -->`);
  return lines.join("\n");
}

function buildRunUrl() {
  const server = process.env.GITHUB_SERVER_URL;
  const repo = process.env.GITHUB_REPOSITORY;
  const runId = process.env.GITHUB_RUN_ID;
  if (server && repo && runId) return `${server}/${repo}/actions/runs/${runId}`;
  return null;
}

/**
 * Fetches all open issues with the `snyk` label and returns
 * a Map<fingerprint, LinearIssue>.
 */
async function fetchOpenSnykIssues(client) {
  const issueMap = new Map();

  const issues = await client.issues({
    filter: {
      team: { id: { eq: TEAM_ID } },
      labels: { name: { eq: "snyk" } },
      state: { type: { nin: ["completed", "canceled"] } },
    },
    first: 250,
  });

  for (const issue of issues.nodes) {
    const match = issue.description?.match(FINGERPRINT_RE);
    if (match) {
      issueMap.set(match[1], issue);
    }
  }

  return issueMap;
}

async function getIssueLabelIds(issue) {
  const labels = await issue.labels();
  return labels.nodes.map((l) => l.id);
}

// --- Core logic ---

async function createIssue(client, finding) {
  await client.createIssue({
    teamId: TEAM_ID,
    projectId: PROJECT_ID,
    stateId: STATE_BACKLOG,
    title: `[Snyk] ${finding.ruleId}: ${truncate(finding.message, 80)}`,
    description: buildDescription(finding),
    priority: mapPriority(finding.level, finding.priorityScore),
    labelIds: [LABEL_SECURITY, LABEL_BACKEND, LABEL_INFRA, LABEL_SNYK],
  });
}

async function handleReappearance(client, issue) {
  const labelIds = await getIssueLabelIds(issue);
  const filtered = labelIds.filter((id) => id !== LABEL_RESOLVED);

  await client.updateIssue(issue.id, { labelIds: filtered });
  await client.createComment({
    issueId: issue.id,
    body: "This vulnerability has reappeared in the latest Snyk scan. Removing `resolved-by-scan` label.",
  });
}

async function markResolving(client, issue) {
  const labelIds = await getIssueLabelIds(issue);
  await client.updateIssue(issue.id, {
    labelIds: [...labelIds, LABEL_RESOLVED],
  });
  await client.createComment({
    issueId: issue.id,
    body: "This vulnerability is no longer detected by Snyk. Marking as `resolved-by-scan` \u2014 will auto-close if still absent in the next scan.",
  });
}

async function confirmResolved(client, issue) {
  await client.updateIssue(issue.id, { stateId: STATE_DONE });
  await client.createComment({
    issueId: issue.id,
    body: "Vulnerability confirmed resolved (absent for two consecutive scans). Auto-closing.",
  });
}

// --- Main ---

async function main() {
  const apiKey = process.env.LINEAR_API_KEY;
  if (!apiKey) {
    console.error("LINEAR_API_KEY is required");
    process.exit(0); // don't fail the build
  }

  const sarifPath = process.argv[2];
  if (!sarifPath) {
    console.error("Usage: node scripts/create-linear-issues.mjs <sarif-file>");
    process.exit(0);
  }

  const client = new LinearClient({ apiKey });
  const findings = parseSarifResults(sarifPath);
  console.log(`Parsed ${findings.size} findings from SARIF`);

  const openIssues = await fetchOpenSnykIssues(client);
  console.log(`Found ${openIssues.size} open snyk-labeled issues in Linear`);

  const stats = { new: 0, tracked: 0, resolving: 0, closed: 0, recurred: 0 };

  // Process each finding
  for (const [fingerprint, finding] of findings) {
    const existing = openIssues.get(fingerprint);

    if (!existing) {
      try {
        await createIssue(client, finding);
        stats.new++;
        console.log(`  NEW: ${finding.ruleId} in ${finding.filePath}`);
      } catch (err) {
        console.error(
          `  Failed to create issue for ${finding.ruleId}: ${err.message}`,
        );
      }
      continue;
    }

    // Existing issue — check if it was marked as resolving and has come back
    try {
      const labelIds = await getIssueLabelIds(existing);
      if (labelIds.includes(LABEL_RESOLVED)) {
        await handleReappearance(client, existing);
        stats.recurred++;
        console.log(`  RECURRED: ${finding.ruleId}`);
      } else {
        stats.tracked++;
      }
    } catch (err) {
      console.error(
        `  Failed to check/update ${existing.identifier}: ${err.message}`,
      );
    }
  }

  // Reconcile: find open issues whose fingerprints are no longer in the scan
  for (const [fingerprint, issue] of openIssues) {
    if (findings.has(fingerprint)) continue;

    try {
      const labelIds = await getIssueLabelIds(issue);
      if (labelIds.includes(LABEL_RESOLVED)) {
        await confirmResolved(client, issue);
        stats.closed++;
        console.log(`  CLOSED: ${issue.identifier} (confirmed resolved)`);
      } else {
        await markResolving(client, issue);
        stats.resolving++;
        console.log(`  RESOLVING: ${issue.identifier}`);
      }
    } catch (err) {
      console.error(
        `  Failed to reconcile ${issue.identifier}: ${err.message}`,
      );
    }
  }

  console.log(
    `\nSummary: ${stats.new} new | ${stats.tracked} tracked | ${stats.resolving} resolving | ${stats.closed} closed | ${stats.recurred} recurred`,
  );
}

main().catch((err) => {
  console.error(`Unexpected error: ${err.message}`);
  process.exit(0); // never fail the build
});
