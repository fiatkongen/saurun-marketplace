# Publish Plugin

Publish plugin changes to GitHub. Syncs to both the private SauRun repo and public saurun-marketplace repo.

Users get updates via `/plugin marketplace update saurun-marketplace` or enable auto-update.

## Usage

```
/publish-plugin [bump]
```

## Parameters

- `bump` (optional): Include to increment the patch version (e.g., 1.0.0 → 1.0.1)

## Steps

**Note:** The command automatically detects your platform (Windows/Mac/Linux) and uses appropriate commands.

1. If `bump` parameter is provided:
   - Read `plugins/saurun/.claude-plugin/plugin.json`
   - Increment the patch version (third number)
   - Update the version in both:
     - `plugins/saurun/.claude-plugin/plugin.json`
     - `.claude-plugin/marketplace.json` (in the plugins array)

2. Stage all plugin-related changes in SauRun (private repo):
   ```bash
   git add .claude-plugin/ plugins/
   ```

3. If there are staged changes in SauRun:
   - Commit with message: `chore(plugin): publish plugin v{version}`
   - Push to origin

4. Sync to public saurun-marketplace repo (platform-specific):

   **On Mac/Linux:**
   ```bash
   # Copy updated files to public repo
   rsync -av --delete ~/repos/SauRun/.claude-plugin/ ~/repos/saurun-marketplace/.claude-plugin/
   rsync -av --delete ~/repos/SauRun/plugins/ ~/repos/saurun-marketplace/plugins/
   ```

   **On Windows:**
   ```powershell
   # Copy updated files to public repo (use \* to copy contents, not folder)
   Copy-Item -Path 'R:\Repos\SauRun\.claude-plugin\*' -Destination 'R:\Repos\saurun-marketplace\.claude-plugin\' -Recurse -Force
   Copy-Item -Path 'R:\Repos\SauRun\plugins\*' -Destination 'R:\Repos\saurun-marketplace\plugins\' -Recurse -Force
   ```

5. Commit and push public repo (platform-specific):

   **On Mac/Linux:**
   ```bash
   cd ~/repos/saurun-marketplace
   git add -A
   git commit -m "chore(plugin): publish plugin v{version}"
   git push origin main
   ```

   **On Windows:**
   ```bash
   cd /r/Repos/saurun-marketplace
   git add -A
   git commit -m "chore(plugin): publish plugin v{version}"
   git push origin main
   ```

6. Report the result:
   - Show new version (if bumped)
   - Confirm both repos pushed successfully
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
