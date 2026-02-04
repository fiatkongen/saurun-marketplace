# Publish Plugin

Publish plugin changes to GitHub (saurun-marketplace repo).

Users get updates via `/plugin marketplace update saurun-marketplace` or enable auto-update.

## Usage

```
/publish-plugin [bump]
```

## Parameters

- `bump` (optional): Include to increment the patch version (e.g., 1.0.0 → 1.0.1)

## Steps

1. If `bump` parameter is provided:
   - Read `plugins/saurun/.claude-plugin/plugin.json`
   - Increment the patch version (third number)
   - Update the version in both:
     - `plugins/saurun/.claude-plugin/plugin.json`
     - `.claude-plugin/marketplace.json` (in the plugins array)

2. Stage all plugin-related changes:
   ```bash
   git add .claude-plugin/ plugins/
   ```

3. If there are staged changes:
   - Commit with message: `chore(plugin): publish plugin v{version}`
   - Push to origin main

4. Report the result:
   - Show new version (if bumped)
   - Confirm repo pushed successfully
   - Remind user: `/plugin marketplace update saurun-marketplace` to refresh

## Examples

```
/publish-plugin        # Push without version bump
/publish-plugin bump   # Bump version and push
```

## First-time Setup

```
/plugin marketplace add fiatkongen/saurun-marketplace
/plugin install saurun@saurun-marketplace
```

## Updating

```
/plugin marketplace update saurun-marketplace
```

Or enable auto-update in `/plugin` settings for automatic updates on startup.
