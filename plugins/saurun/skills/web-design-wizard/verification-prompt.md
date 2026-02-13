# Verification Sub-Agent Prompt Template

Use this template for each style when spawning parallel verification agents in Phase 7.

Replace all `[placeholders]` with actual values.

---

```
Verify the [StyleName] implementation for [ProjectName].

## Location
Repo root: [absolute-path-to-repo-root]
Worktree: [absolute-path-to-repo-root]/.worktrees/[style-name]/
Design spec: [absolute-path-to-repo-root]/design-system/pages/[style-name].md

## Checks

1. **Dev server**
   → cd to worktree
   → Start dev server in background and verify it responds:

   **Unix/macOS:**
   `npm run dev -- --port [assigned-port] & echo $! > .dev-pid && sleep 5 && curl -s http://localhost:[assigned-port] > /dev/null && echo "OK" || echo "FAILED"`

   **Windows (PowerShell):**
   `Start-Process -NoNewWindow npm -ArgumentList "run","dev","--","--port","[assigned-port]" ; Start-Sleep 5 ; try { Invoke-WebRequest -Uri http://localhost:[assigned-port] -UseBasicParsing | Out-Null; "OK" } catch { "FAILED" }`

   → Check output for errors

2. **URL validation**
   → Read src/ files and extract all href/src attributes
   → Verify internal links resolve and images exist
   → Report any broken references

3. **Graphics validation**
   → Check images exist in src/assets/
   → Verify referenced correctly in code
   → Check graphics preference was followed:
     - Lots: Hero + 2-3 accents present
     - Minimal: Hero only
     - Style-determines: Reasonable for the design

4. **Design spec adherence**
   → Read [absolute-path-to-repo-root]/design-system/pages/[style-name].md
   → Score each (1-10): color palette, typography, layout, effects
   → Note significant deviations

5. **Production build**
   → npm run build
   → Must succeed, note bundle size

## Output Format

[StyleName] Verification Report
━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅/❌ Dev server: [status]
✅/❌ URLs: [X/Y valid] [list broken if any]
✅/❌ Graphics: [X/Y present] [issues if any]
✅/❌ Design adherence: [avg score]/10
✅/❌ Production build: [status] [bundle size]

Issues found: [list or "None"]

## Important
- After all checks complete, stop the dev server:
  - **Unix/macOS:** `kill $(cat .dev-pid) && rm .dev-pid`
  - **Windows:** `npx kill-port [assigned-port]`
- The orchestrator assigns ports: style 1 → 5173, style 2 → 5174, style 3 → 5175, etc.
```
