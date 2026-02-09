Complete deployment workflow:

Step 1 - Validate: test -f SKILL.md, head -n 20 for frontmatter validation. Abort if missing.
Step 2 - Check marketplace: git status, stat -f for modification times, diff -u if marketplace newer. Warn + ask confirmation.
Step 3 - Copy: uname detection → Darwin → rsync -av --delete. Full directory copy.
Step 4 - Publish: /publish-plugin bump (version bump, commit, push).

Safety checks listed with ✅ markers. Complete command sequence at end. Never skipped under time pressure.
