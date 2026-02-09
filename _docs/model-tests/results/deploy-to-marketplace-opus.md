Complete deployment workflow:

Step 1 - Validate: ls SKILL.md, head -30 to check frontmatter (name + description). Abort if missing.
Step 2 - Check marketplace: git status for uncommitted changes, stat -f to compare modification times, diff -ru if conflict. Warn + ask user confirmation.
Step 3 - Copy: rsync -av --delete (Darwin detected). Copies entire directory.
Step 4 - Publish: /publish-plugin bump (version bump, commit, push).

Safety checks summary table with abort/warn actions. Two explicit user confirmation points (uncommitted changes, newer marketplace version).
