"""WorktreeCreate hook: creates a git worktree and installs dependencies.

Replaces Claude Code's default git worktree behavior to add post-creation
setup (npm install for the React client).

Input (stdin JSON):
  - name: slug identifier for the worktree
  - cwd: current working directory (repo root)

Output (stdout):
  - Absolute path to the created worktree directory
"""

import json
import os
import shutil
import subprocess
import sys


def main():
    payload = json.loads(sys.stdin.read())
    name = payload["name"]
    cwd = payload["cwd"]

    # If cwd doesn't exist (e.g., stale worktree path), resolve the repo root
    if not os.path.isdir(cwd):
        try:
            cwd = subprocess.run(
                ["git", "rev-parse", "--show-toplevel"],
                capture_output=True, text=True, check=True,
            ).stdout.strip()
        except subprocess.CalledProcessError:
            print(f"ERROR: cwd {cwd} does not exist and git toplevel not found", file=sys.stderr)
            sys.exit(1)

    worktree_dir = os.path.join(cwd, ".claude", "worktrees", name)
    branch_name = f"worktree-{name}"

    # Create the git worktree (redirect output to stderr so stdout stays clean)
    subprocess.run(
        ["git", "worktree", "add", worktree_dir, "-b", branch_name],
        cwd=cwd,
        check=True,
        stdout=sys.stderr,
        stderr=sys.stderr,
    )

    # Install npm dependencies if the client directory exists
    client_dir = os.path.join(worktree_dir, "src", "client")
    package_json = os.path.join(client_dir, "package.json")

    if os.path.exists(package_json):
        npm = shutil.which("npm")
        if npm:
            print(f"Installing npm dependencies in {client_dir}...", file=sys.stderr)
            subprocess.run(
                [npm, "install"],
                cwd=client_dir,
                check=True,
                stdout=sys.stderr,
                stderr=sys.stderr,
            )
        else:
            print("WARNING: npm not found in PATH, skipping install", file=sys.stderr)

    # Print the worktree path for Claude Code (must be the only stdout)
    print(worktree_dir)


if __name__ == "__main__":
    main()
