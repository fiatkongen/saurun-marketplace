# Publish Plugin

Publish plugin changes to GitHub (saurun-marketplace repo).

Users get updates via `/plugin marketplace update saurun-marketplace` or enable auto-update.

## Usage

```
/publish-plugin [--no-bump]
```

## Parameters

- `--no-bump` (optional): Skip version increment (rarely needed)

## Steps

1. **Bump version** (unless `--no-bump` is provided):
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
   - Confirm repo pushed

## Examples

```
/publish-plugin           # Bump version and push (default)
/publish-plugin --no-bump # Push without version bump (rare)
```

## First-time Setup

```
/plugin marketplace add fiatkongen/saurun-marketplace
/plugin install saurun@saurun-marketplace
```

## Updating

To refresh your local plugin cache manually:

```
/plugin marketplace update saurun-marketplace
```
