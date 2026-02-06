---
name: design-polish
description: >
  Use when a frontend has placeholder assets (data-asset attributes, empty image slots)
  and needs real generated visuals. Triggers: post-implementation polish, preparing for
  demo/launch, replacing placeholder images with AI-generated assets matching a design system.
user-invocable: true
argument-hint: "[spec-file-path] (optional)"
---

# Design Polish (Asset Generation)

Replace placeholder assets in a React frontend with AI-generated visuals that match the project's design system. Uses `nanobanana-skill` for image generation.

## When to Use

- Frontend has `data-asset` placeholder attributes ready for replacement
- Design system (`design-system/MASTER.md`) exists with style/color definitions
- Preparing for demo, launch, or stakeholder review
- Post-implementation visual polish pass

## When NOT to Use

- No frontend UI exists yet
- No design system defined (create one first)
- Need hand-crafted assets (use a designer)

## Inputs

Resolved automatically. Override with `$ARGUMENTS` if provided.

| Input | Auto-Detection | Fallback |
|-------|---------------|----------|
| Spec | `$ARGUMENTS` or `_docs/*spec*`, `**/SPEC.md` | Skip spec-driven inventory, use placeholders only |
| Design system | `design-system/MASTER.md` | **Required** — abort if missing |
| Frontend root | `frontend/src/` | Scan for `src/` with React components |

## Process

### 1. Resolve Inputs

```
If $ARGUMENTS provided → use as spec path
Else → search for _docs/*spec*, **/SPEC.md
Read design-system/MASTER.md → extract style name, mood, palette, aesthetic
Locate frontend source root
```

### 2. Asset Inventory

Build inventory from two sources:

**From spec** (if available):

| Asset Type | Where to Look in Spec |
|------------|----------------------|
| Hero images | "## Solution" + landing page sections |
| Empty states | "## User Flows" — zero-data scenarios |
| Error states | Standard: 404, 500, network error |
| Success states | Features completing actions |
| Marketing | OG image (1200x630), favicon (32/192/512px) |

**From code** (always):

Scan frontend source for `data-asset="{type}-{name}"` attributes. Each placeholder maps to an inventory item. Flag any placeholders that do not match a spec-driven inventory item as "unmatched placeholders" in the inventory file.

Write inventory to `_docs/design-polish/asset-inventory.md`.

### 3. Generate Assets

For each asset in inventory:

1. Read MASTER.md for style context
2. Construct prompt:
   ```
   "{style_name} aesthetic. {mood_keywords} mood.
    Colors: {palette}.
    Generate: {asset_description}.
    Dimensions: {width}x{height}.
    Format: PNG transparent (illustrations) / JPG (photos).
    Context: {product_description} for {target_users}."
   ```
3. Invoke `nanobanana-skill` via Skill tool with the prompt
4. Save to `frontend/public/assets/{category}/{filename}`
5. Verify file exists and has expected dimensions
6. Log to `_docs/design-polish/generation-log.md`
7. If fails after 2 retries → mark "manual needed", continue

### 4. Wire Assets to Components

For each `data-asset` placeholder found:
- Replace placeholder div with `<img>` tag:
  ```tsx
  <img src="/assets/{category}/{filename}" alt="{description}" className="..." />
  ```

Update `index.html`:
- `<meta property="og:image" content="/assets/marketing/og-image.jpg" />`
- `<link rel="icon" href="/assets/icons/favicon-32.png" />`

### 5. Verify

- Check for remaining placeholders: search for `data-asset=` in frontend source
- If E2E tests exist: run `cd frontend && npx playwright test`
- If E2E fails → debug and fix (up to 2 attempts)

## Asset Directory Structure

```
frontend/public/assets/
├── heroes/
│   └── landing-hero.jpg
├── illustrations/
│   ├── empty-state.png
│   ├── error-state.png
│   └── success-state.png
├── icons/
│   ├── favicon-32.png
│   ├── favicon-192.png
│   └── favicon-512.png
└── marketing/
    └── og-image.jpg
```

## Output

- `_docs/design-polish/asset-inventory.md` — what was planned
- `_docs/design-polish/generation-log.md` — what was generated (with prompts used)
- `frontend/public/assets/` — populated with generated images
- Components updated with real `<img>` tags
- No `data-asset=` placeholders remaining

## Completion Checklist

- [ ] Asset inventory exists at `_docs/design-polish/asset-inventory.md`
- [ ] Generation log exists at `_docs/design-polish/generation-log.md`
- [ ] All hero images generated (or marked manual)
- [ ] All illustration assets generated (or marked manual)
- [ ] OG image + favicon generated
- [ ] No `data-asset=` placeholders remaining in code
- [ ] E2E tests still pass (if they exist)

## Common Mistakes

- **Generating before reading MASTER.md** — assets won't match the design system. Always extract style context first.
- **Hardcoding asset paths** — use the `{category}/{filename}` convention so paths stay consistent.
- **Skipping failed assets** — mark them "manual needed" in the log, don't silently skip.
