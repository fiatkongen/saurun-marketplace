# Default Design System — Hovemunk

Skandinavisk simpel, varme toner, Linear-inspireret. Dark + light mode.

Brug dette som `_docs/design.md` når ingen projekt-specifik design er givet.

---

## Æstetik

**Stil:** Skandinavisk minimal med varme undertoner
**Inspiration:** Linear, Vercel, Stripe dashboard
**Princip:** Roligt, fokuseret, professionelt. Ingen clutter. Hvert element har en grund til at eksistere.

## CSS Custom Properties (shadcn/ui kompatibel)

Disse variabler bruges af shadcn/ui components. **ALTID definer disse i global CSS.**

```css
:root {
  --background: 48 20% 98%;           /* #FAFAF8 */
  --foreground: 240 5% 10%;           /* #1A1A1B */
  --card: 0 0% 100%;                  /* #FFFFFF */
  --card-foreground: 240 5% 10%;
  --popover: 40 14% 96%;              /* #F5F5F3 */
  --popover-foreground: 240 5% 10%;
  --primary: 239 84% 57%;             /* #4F46E5 */
  --primary-foreground: 0 0% 100%;
  --secondary: 40 10% 94%;
  --secondary-foreground: 240 5% 10%;
  --muted: 40 10% 94%;
  --muted-foreground: 0 0% 42%;       /* #6B6B6B */
  --accent: 40 14% 96%;
  --accent-foreground: 240 5% 10%;
  --destructive: 0 72% 51%;           /* #DC2626 */
  --destructive-foreground: 0 0% 100%;
  --border: 40 7% 90%;                /* #E5E5E3 */
  --input: 40 7% 90%;
  --ring: 239 84% 57%;
  --radius: 0.375rem;
  --success: 160 84% 39%;             /* #059669 */
  --success-foreground: 0 0% 100%;
  --warning: 38 92% 50%;              /* #D97706 */
  --warning-foreground: 0 0% 100%;
  --info: 217 91% 60%;                /* #2563EB */
  --info-foreground: 0 0% 100%;
}

.dark {
  --background: 240 7% 4%;            /* #0A0A0B */
  --foreground: 40 7% 96%;            /* #F5F5F3 */
  --card: 240 4% 8%;                  /* #141415 */
  --card-foreground: 40 7% 96%;
  --popover: 240 3% 12%;              /* #1C1C1E */
  --popover-foreground: 40 7% 96%;
  --primary: 239 84% 67%;             /* #6366F1 */
  --primary-foreground: 0 0% 100%;
  --secondary: 240 3% 12%;
  --secondary-foreground: 40 7% 96%;
  --muted: 240 3% 12%;
  --muted-foreground: 0 0% 61%;       /* #9B9B9B */
  --accent: 240 3% 12%;
  --accent-foreground: 40 7% 96%;
  --destructive: 0 84% 71%;           /* #F87171 */
  --destructive-foreground: 0 0% 100%;
  --border: 240 4% 17%;               /* #2A2A2D */
  --input: 240 4% 17%;
  --ring: 239 84% 67%;
  --success: 160 60% 64%;             /* #34D399 */
  --success-foreground: 0 0% 5%;
  --warning: 45 93% 56%;              /* #FBBF24 */
  --warning-foreground: 0 0% 5%;
  --info: 213 94% 68%;                /* #60A5FA */
  --info-foreground: 0 0% 5%;
}
```

## Tailwind v4 Theme

Tilføj i `src/index.css` efter `@import "tailwindcss"`:

```css
@theme {
  --color-background: var(--background);
  --color-surface: #141415;
  --color-surface-elevated: #1C1C1E;
  --color-border: #2A2A2D;
  --color-accent: #6366F1;
  --color-accent-hover: #818CF8;
  --color-success: #34D399;
  --color-warning: #FBBF24;
  --color-danger: #F87171;
  --color-info: #60A5FA;
  --font-sans: "Inter", sans-serif;
  --font-mono: "JetBrains Mono", monospace;
}
```

## Farver — Quick Reference

### Dark Mode (default)

| Rolle | Hex | Brug |
|-------|-----|------|
| Background | `#0A0A0B` | App baggrund |
| Surface | `#141415` | Kort, paneler, sidebar |
| Surface elevated | `#1C1C1E` | Modals, dropdowns, hover states |
| Border | `#2A2A2D` | Separatorer, kort-kanter |
| Text primary | `#F5F5F3` | Overskrifter, primær tekst |
| Text secondary | `#9B9B9B` | Labels, hjælpetekst |
| Text tertiary | `#636366` | Placeholders, deaktiveret |
| Accent | `#6366F1` | Primære knapper, links, fokus |
| Accent hover | `#818CF8` | Hover state |
| Success | `#34D399` | Positive states |
| Warning | `#FBBF24` | Advarsler |
| Danger | `#F87171` | Fejl, destruktive handlinger |
| Info | `#60A5FA` | Informativ |

### Light Mode

| Rolle | Hex |
|-------|-----|
| Background | `#FAFAF8` |
| Surface | `#FFFFFF` |
| Surface elevated | `#F5F5F3` |
| Border | `#E5E5E3` |
| Text primary | `#1A1A1B` |
| Text secondary | `#6B6B6B` |
| Text tertiary | `#9CA3AF` |
| Accent | `#4F46E5` |
| Accent hover | `#4338CA` |
| Success | `#059669` |
| Warning | `#D97706` |
| Danger | `#DC2626` |
| Info | `#2563EB` |

## Typografi

| Element | Font | Størrelse | Vægt | Line height |
|---------|------|-----------|------|-------------|
| H1 | Inter | 1.75rem (28px) | 600 | 1.2 |
| H2 | Inter | 1.375rem (22px) | 600 | 1.3 |
| H3 | Inter | 1.125rem (18px) | 600 | 1.4 |
| Body | Inter | 0.875rem (14px) | 400 | 1.6 |
| Body small | Inter | 0.8125rem (13px) | 400 | 1.5 |
| Caption | Inter | 0.75rem (12px) | 500 | 1.4 |
| Mono/code | JetBrains Mono | 0.8125rem (13px) | 400 | 1.5 |

**NB:** HTML root er 16px. Body bruger `text-sm` (14px). Tekst under 12px er forbudt.

**Font loading:**
```html
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500&display=swap" rel="stylesheet">
```

## Spacing

4px grid:
| Token | Størrelse | Brug |
|-------|----------|------|
| `1` | 4px | Tight (ikon + label) |
| `2` | 8px | Compact (relaterede elementer) |
| `3` | 12px | Default padding i kort |
| `4` | 16px | Comfortable (sektioner) |
| `6` | 24px | Mellem sektioner |
| `8` | 32px | Mellem store blokke |
| `10` | 40px | Page-level padding |
| `12` | 48px | Page sections |

## Border Radius

| Element | Radius | Tailwind |
|---------|--------|----------|
| Buttons | 6px | `rounded-md` |
| Cards/panels | 8px | `rounded-lg` |
| Modals | 12px | `rounded-xl` |
| Badges/tags | 4px | `rounded` |
| Avatars | 9999px | `rounded-full` |
| Global (--radius) | 6px | — |

## Shadows

Brug sparsomt. Kun på elevated elements.

**Dark mode:**
```css
/* Card */ box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
/* Modal */ box-shadow: 0 8px 32px rgba(0, 0, 0, 0.5);
```

**Light mode:**
```css
/* Card */ box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
/* Modal */ box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
```

## Z-Index Scale

| Layer | z-index | Brug |
|-------|---------|------|
| Base | 0 | Normal content |
| Sticky | 10 | Sticky headers |
| Dropdown | 50 | Dropdown menus, popovers |
| Modal overlay | 100 | Modal backdrop |
| Modal | 110 | Modal content |
| Toast | 200 | Toast notifications |

## Animationer

- **Durations:** 150ms (micro), 200ms (standard), 300ms (page transitions)
- **Easing:** `cubic-bezier(0.4, 0, 0.2, 1)` (standard), `cubic-bezier(0, 0, 0.2, 1)` (decelerate)
- **Hover:** `opacity`, `background-color`, `transform: scale(1.02)` — aldrig `width`/`height`
- **Respektér:** `prefers-reduced-motion: reduce`

## Ikoner

- **Library:** Lucide React (`lucide-react`)
- **Størrelse:** 16px (inline), 20px (buttons), 24px (navigation)
- **Stroke width:** 1.5px
- **Farve:** Følg text-farven

## Accessibility

### Focus Ring
```css
focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background
```
Alle interaktive elementer SKAL have synlig focus ring.

### Kontrast
- Normal tekst: minimum 4.5:1 ratio
- Stor tekst (>18px): minimum 3:1 ratio
- **Light mode semantic farver er mørkere** for at overholde kontrast (se light mode tabel)

### Touch targets
Minimum 44x44px for mobile touch targets.

## Knapper

Brug shadcn/ui `<Button>` component med themed farver.

| Variant | Brug |
|---------|------|
| `default` | Primær handling |
| `secondary` | Sekundær handling |
| `ghost` | Tertiær, diskret handling |
| `destructive` | Slet, fjern, farlig handling |
| `outline` | Alternative sekundær |

**Størrelser:**
| Size | Tailwind | Brug |
|------|----------|------|
| `sm` | `h-7 px-3 text-xs` | Inline, tabeller |
| `default` | `h-9 px-4 text-sm` | Standard |
| `lg` | `h-11 px-6 text-base` | Hero, prominent CTA |

**Loading state:** Disable button + vis spinner icon under async operationer.

## Form Elements

Brug shadcn/ui components: `<Input>`, `<Label>`, `<Select>`, `<Checkbox>`, `<Switch>`, `<Textarea>`.

| Element | Spec |
|---------|------|
| Input height | `h-9` (36px) |
| Input padding | `px-3` |
| Input border | `border border-input` (brug CSS var) |
| Input focus | `focus-visible:ring-2 focus-visible:ring-ring` |
| Input placeholder | Text tertiary farve |
| Label | `text-sm font-medium`, 4px over input |
| Error message | `text-sm text-destructive`, 4px under input |
| Disabled | `opacity-50 cursor-not-allowed` |

### Form Layout
- Labels over inputs (ikke inline)
- 16px spacing mellem form groups
- Error messages vises inline under feltet
- Submit knap har 24px spacing over sig

## Tables

Brug shadcn/ui `<Table>` component.

| Element | Spec |
|---------|------|
| Header bg | `bg-muted/50` |
| Header text | `text-sm font-medium text-muted-foreground` |
| Row height | Minimum `h-12` |
| Row hover | `hover:bg-muted/50` |
| Cell padding | `px-4 py-3` |
| Border | `border-b border-border` mellem rækker |
| Sortable | Lucide `ArrowUpDown` ikon i header |

## States

### Loading
- **Skeleton screens** foretrækkes over spinners
- Skeleton farve: `bg-muted animate-pulse rounded`
- Spinner: Lucide `Loader2` med `animate-spin`, accent farve
- **Loading buttons:** Disable + erstatte tekst med spinner

### Empty States
- Centrer vertikalt og horisontalt
- Lucide ikon i 48px, text-muted-foreground
- Heading: `text-lg font-medium`
- Description: `text-sm text-muted-foreground`, max 45ch bredde
- Optional CTA knap under

### Error States
- **Inline field errors:** `text-sm text-destructive`, 4px under input
- **Toast notifications:** Brug shadcn `<Toaster>`, position top-right, auto-dismiss 5s
- **Full-page error (404/500):** Centrer, stor ikon, heading + description + "Gå tilbage" knap

### Disabled
- `opacity-50 cursor-not-allowed pointer-events-none`

## Navigation / Sidebar

| Element | Spec |
|---------|------|
| Width | 240px (expanded), 60px (collapsed, icon-only) |
| Background | `bg-card` |
| Border | `border-r border-border` |
| Item height | `h-9` |
| Item padding | `px-3` |
| Item active | `bg-accent text-accent-foreground` |
| Item hover | `hover:bg-accent/50` |
| Section header | `text-xs font-medium text-muted-foreground uppercase tracking-wider`, `px-3 py-2` |
| Mobile | Sidebar hidden, hamburger menu → Sheet overlay |

## shadcn Components (base install)

Installér disse ved scaffold:
```bash
npx shadcn@latest add button card input label select separator sheet skeleton table tabs toast tooltip
```

Tilføj flere efter behov under implementering.

## Layout

- **Max content width:** 1280px (`max-w-7xl`)
- **Mobile-first:** Responsive design, `sm:` → `md:` → `lg:` → `xl:`
- **Breakpoints:** sm: 640px, md: 768px, lg: 1024px, xl: 1280px

## Anti-patterns (undgå)

- ❌ Emojis som ikoner — brug Lucide SVG ikoner
- ❌ Gradient backgrounds — hold det fladt
- ❌ Rounded corners > 12px (undtagen avatarer)
- ❌ Mere end 2 font weights på samme side
- ❌ Colored borders — brug subtle grå
- ❌ Box shadows på alt — kun elevated elements
- ❌ Animations > 300ms
- ❌ Text under 12px
- ❌ Inline styles — brug Tailwind klasser eller CSS vars
- ❌ Hardcoded farve-hex i components — brug CSS custom properties
