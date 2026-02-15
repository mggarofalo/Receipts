#!/usr/bin/env node

/**
 * Script to create Linear issues from Snyk vulnerability findings
 * Usage: node scripts/create-linear-issues.mjs <snyk-json-file>
 *
 * Required environment variables:
 * - LINEAR_API_KEY: Linear API key
 * - LINEAR_TEAM_ID: Linear team ID
 * - LINEAR_PROJECT_ID: Linear project ID (optional)
 */

import { readFileSync } from 'fs';
import { execSync } from 'child_process';
import https from 'https';

// Configuration from environment variables
const LINEAR_API_KEY = process.env.LINEAR_API_KEY;
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

    // Handle Snyk Code Test results (source code analysis) - SARIF format
    if (results.runs && results.runs[0]?.results) {
      const resultsArray = Array.isArray(results.runs[0].results) ? results.runs[0].results : [];

      for (const vuln of resultsArray) {
        const severity = vuln.level || vuln.properties?.priorityScore || 'medium';
        const severityLower = typeof severity === 'string' ? severity.toLowerCase() : 'medium';

        // Check severity by level or priority score
        const isSevere = ['critical', 'high', 'medium'].includes(severityLower) ||
                        (vuln.properties?.priorityScore && vuln.properties.priorityScore >= 300) || // Include medium+ severity by priority score
                        (typeof severity === 'number' && severity >= 7); // High numeric severity scores

        if (isSevere) {
          const location = vuln.locations?.[0]?.physicalLocation;
          const filePath = location?.artifactLocation?.uri || 'Unknown file';
          const line = location?.region?.startLine || 'Unknown line';

          vulnerabilities.push({
            title: `Security Vulnerability: ${vuln.ruleId || 'Unknown'}`,
            description: `
**Severity:** ${severity}
**File:** ${filePath}
**Line:** ${line}
**Rule ID:** ${vuln.ruleId || 'Unknown'}
**Message:** ${vuln.message?.text || 'Security vulnerability detected'}

**Description:**
${vuln.message?.text || 'A security vulnerability was detected in the code.'}

**Finding ID:** ${vuln.properties?.['snyk/finding-id'] || 'N/A'}
            `,
            severity: severityLower
          });
        }
      }
    }

    // Handle Snyk Open Source results (dependency scanning) - fallback
    if (vulnerabilities.length === 0 && results.vulnerabilities) {
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
  return new Promise((resolve, reject) => {
    const title = `[Security] ${vulnerability.title}`;
    const description = vulnerability.description;
    const priority = SEVERITY_TO_PRIORITY[vulnerability.severity] || 3;

    const query = `
      mutation IssueCreate($input: IssueCreateInput!) {
        issueCreate(input: $input) {
          success
          issue {
            id
            number
            title
          }
        }
      }
    `;

    const variables = {
      input: {
        teamId: TEAM_ID,
        title: title,
        description: description,
        priority: priority,
        labelIds: [], // Could add security/backend labels if we look them up
        ...(PROJECT_ID && { projectId: PROJECT_ID })
      }
    };

    const postData = JSON.stringify({
      query: query,
      variables: variables
    });

    const options = {
      hostname: 'api.linear.app',
      port: 443,
      path: '/graphql',
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Content-Length': Buffer.byteLength(postData),
        'Authorization': LINEAR_API_KEY
      }
    };

    const req = https.request(options, (res) => {
      let data = '';

      res.on('data', (chunk) => {
        data += chunk;
      });

      res.on('end', () => {
        try {
          const response = JSON.parse(data);

          if (response.errors) {
            console.error('Linear API error:', response.errors);
            resolve(false);
            return;
          }

          if (response.data?.issueCreate?.success) {
            const issue = response.data.issueCreate.issue;
            console.log(`Created Linear issue: ${issue.title} (#${issue.number})`);
            resolve(true);
          } else {
            console.error('Failed to create Linear issue:', response);
            resolve(false);
          }
        } catch (error) {
          console.error('Error parsing Linear response:', error);
          resolve(false);
        }
      });
    });

    req.on('error', (error) => {
      console.error('Error creating Linear issue:', error);
      resolve(false);
    });

    req.write(postData);
    req.end();
  });
}

async function main() {
  // Validate required environment variables
  if (!LINEAR_API_KEY) {
    console.error('Error: LINEAR_API_KEY environment variable is required');
    process.exit(1);
  }
  if (!TEAM_ID) {
    console.error('Error: LINEAR_TEAM_ID environment variable is required');
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
    if (await createLinearIssue(vuln)) {
      created++;
    }
  }

  console.log(`Created ${created} Linear issues.`);
}

if (import.meta.url === `file://${process.argv[1]}`) {
  main();
}