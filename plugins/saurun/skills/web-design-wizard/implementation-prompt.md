# Implementation Sub-Agent Prompt Template

Use this template for each style when spawning parallel implementation agents in Phase 6.

Replace all `[placeholders]` with actual values.

---

```
Implement the [StyleName] design for [ProjectName].

## Context
- Repo root: [absolute-path-to-repo-root]
- Design specification: [absolute-path-to-repo-root]/design-system/pages/[style-name].md
- Graphics preference: [lots/minimal/style-determines]
- Content language: [English/Danish/etc.]
- Concept document: [absolute-path-to-repo-root]/designs/[project-name]/concept.md

## Steps

1. Invoke the Skill tool with skill "superpowers:using-git-worktrees"
   → Create worktree at: .worktrees/[style-name]/

2. Create React + Vite + TypeScript project in the worktree:
   → npm create vite@latest . -- --template react-ts
   → npm install

3. Read the design specification: [absolute-path-to-repo-root]/design-system/pages/[style-name].md

4. Invoke the Skill tool with skill "frontend-design"
   → Implement the full design system into the React app
   → Apply production-grade polish per frontend-design guidelines

5. Invoke the Skill tool with skill "vercel-react-best-practices"
   → Optimize React performance and bundle size

6. Invoke the Skill tool with skill "copywriting"
   → Write and refine all copy to match design tone/voice
   → Content must be in [content-language]

7. Invoke the Skill tool with skill "nano-banana-pro"
   → Generate images based on graphics preference:
     - Lots: Hero (16:9) + 2-3 accent graphics (4:3, 1:1)
     - Minimal: Hero image only (16:9)
     - Style-determines: Based on design style needs
   → Use prompts from design specification
   → Save to src/assets/

8. Verify production build:
   → npm run build
   → Must succeed without errors

9. Commit all changes:
   → git add src/ public/ index.html package.json tsconfig*.json vite.config.ts
   → git commit -m "feat: implement [style-name] design for [project-name]"

## Important
- Do NOT invoke ui-ux-pro-max — the design spec is already generated
- If a skill is not available, skip that step and note it in your output
- Focus on implementation quality, not spec regeneration
```
