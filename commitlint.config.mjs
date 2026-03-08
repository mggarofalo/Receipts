export default {
  extends: ["@commitlint/config-conventional"],
  rules: {
    // Allowed scopes matching the project's Conventional Commits convention
    // (see AGENTS.md § Commit Convention)
    "scope-enum": [
      2,
      "always",
      [
        "api",
        "client",
        "domain",
        "application",
        "infrastructure",
        "infra",
        "common",
        "shared",
        "ci",
        "hooks",
      ],
    ],
    // Allow empty scopes — not every commit needs a scope
    "scope-empty": [0],
    // Enforce max header length of 100 characters
    "header-max-length": [2, "always", 100],
  },
};
