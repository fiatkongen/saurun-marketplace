## Test Deploy Skill Flow

**Purpose:** Validate the deploy-to-marketplace workflow

**Expected Actions:**

1. **Copy Skills Correctly**
   - Verify skills from `plugins/saurun/skills/` are properly copied to marketplace staging area
   - Check directory structure matches expected layout

2. **Validate Structure**
   - Confirm each skill has required `SKILL.md` with YAML frontmatter (name, description)
   - Validate optional directories if present (references/, phases/, etc.)
   - Check skill naming conventions

3. **Handle Version Bumping**
   - Verify version in `plugins/saurun/.claude-plugin/plugin.json` is in sync with `.claude-plugin/marketplace.json`
   - Test patch version increment logic (if bump flag provided)
   - Confirm both files update consistently

4. **Commits and Pushes**
   - Stage `.claude-plugin/` and `plugins/` directories
   - Create commit with message: `chore(plugin): publish plugin v{version}`
   - Push to `origin main`

**Test Validation Points:**
- Marketplace manifest is valid JSON
- Plugin manifest matches marketplace version
- All required skill files present
- Git operations succeed without errors
- Commit history shows proper message format

This skill would essentially perform a dry-run or validation of what the `/publish-plugin` command does in production.
