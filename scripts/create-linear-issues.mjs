#!/usr/bin/env node

/**
 * Script to create Linear issues from Snyk vulnerability findings
 * Usage: node scripts/create-linear-issues.mjs <snyk-json-file>
 *
 * Required environment variables:
 * - LINEAR_TEAM_ID: Linear team ID
 * - LINEAR_PROJECT_ID: Linear project ID
 */

import { readFileSync } from 'fs';
import { execSync } from 'child_process';

// Configuration from environment variables
const TEAM_ID = process.env.LINEAR_TEAM_ID;
const PROJECT_ID = process.env.LINEAR_PROJECT_ID;

// Severity mapping to Linear priority
const SEVERITY_TO_PRIORITY = {
  critical: 1, // Urgent
  high: 2,     // High
  medium: 3,   // Medium
  low: 4       // Low
};

function parseSnykResults(jsonFile) {
  try {
    const data = readFileSync(jsonFile, 'utf8');
    const results = JSON.parse(data);

    const vulnerabilities = [];

    // Snyk JSON structure varies, but typically has vulnerabilities array
    if (results.vulnerabilities) {
      for (const vuln of results.vulnerabilities) {
        if (['critical', 'high', 'medium'].includes(vuln.severity)) {
          vulnerabilities.push({
            title: `${vuln.packageName}: ${vuln.title}`,
            description: `
**Severity:** ${vuln.severity}
**Package:** ${vuln.packageName} ${vuln.version}
**Vulnerability:** ${vuln.title}
**CVE:** ${vuln.identifiers?.CVE?.join(', ') || 'N/A'}
**CVSS Score:** ${vuln.cvssScore || 'N/A'}

**Description:**
${vuln.description}

**Remediation:**
${vuln.remediation || 'No remediation available'}

**References:**
${vuln.references?.map(ref => `- ${ref.url}`).join('\n') || 'None'}
            `,
            severity: vuln.severity,
            package: vuln.packageName,
            cve: vuln.identifiers?.CVE?.[0] || null
          });
        }
      }
    }

    return vulnerabilities;
  } catch (error) {
    console.error('Error parsing Snyk results:', error);
    return [];
  }
}

function createLinearIssue(vulnerability) {
  const title = `[Security] ${vulnerability.title}`;
  const description = vulnerability.description;
  const priority = SEVERITY_TO_PRIORITY[vulnerability.severity] || 3;

  // Using Linear MCP to create issue
  // This assumes Linear MCP is available and configured
  const linearCommand = `linear issue create --team "${TEAM_ID}" --project "${PROJECT_ID}" --title "${title}" --description "${description}" --priority ${priority} --label "security" --label "backend"`;

  try {
    console.log(`Creating Linear issue: ${title}`);
    const result = execSync(linearCommand, { encoding: 'utf8' });
    console.log('Issue created:', result);
    return true;
  } catch (error) {
    console.error('Failed to create Linear issue:', error);
    return false;
  }
}

function main() {
  // Validate required environment variables
  if (!TEAM_ID) {
    console.error('Error: LINEAR_TEAM_ID environment variable is required');
    process.exit(1);
  }
  if (!PROJECT_ID) {
    console.error('Error: LINEAR_PROJECT_ID environment variable is required');
    process.exit(1);
  }

  const jsonFile = process.argv[2];

  if (!jsonFile) {
    console.error('Usage: node scripts/create-linear-issues.mjs <snyk-json-file>');
    process.exit(1);
  }

  console.log('Parsing Snyk results from:', jsonFile);
  const vulnerabilities = parseSnykResults(jsonFile);

  if (vulnerabilities.length === 0) {
    console.log('No critical/high/medium vulnerabilities found.');
    return;
  }

  console.log(`Found ${vulnerabilities.length} vulnerabilities to report.`);

  let created = 0;
  for (const vuln of vulnerabilities) {
    if (createLinearIssue(vuln)) {
      created++;
    }
  }

  console.log(`Created ${created} Linear issues.`);
}

if (import.meta.url === `file://${process.argv[1]}`) {
  main();
}