---
name: react-tailwind-v4-components
description: Use when building React components with Tailwind CSS v4, shadcn/ui, or Radix primitives. Use when writing className strings, cva variants, or arbitrary CSS variable values in Tailwind. Critical for avoiding v3 syntax that silently breaks in v4.
---

# React Components with Tailwind v4 + shadcn/ui

## Overview

Reference for building React components with Tailwind CSS v4, shadcn/ui, and Radix. **Primary purpose:** prevent v3 syntax habits that silently break in v4.

## Tailwind v4 Syntax — Critical Changes

### CSS Variable Syntax: Parentheses, NOT Brackets

```tsx
// v4 CORRECT — parentheses for READING CSS variables
<div className="bg-(--brand-color) text-(--text-color) ring-(--accent)">

// v3 WRONG — square brackets
<div className="bg-[--brand-color] text-[var(--text-color)]">
```

**This is the #1 mistake.** Square bracket CSS variable syntax silently fails in v4.

**This applies EVERYWHERE — including inside cva() strings and arbitrary values.**

### Opacity Modifiers with CSS Variables

Opacity modifiers work with parentheses syntax:

```tsx
<div className="bg-(--brand-color)/10 border-(--accent)/50 text-(--heading)/80">
```

### Setting CSS Variables Per Variant

**`theme()` is gone for arbitrary values.** Use `var(--color-*)` directly. Tailwind v4 exposes all theme values as CSS variables: `--color-blue-500`, `--spacing-4`, `--radius-lg`, etc.

**Option A: data-attribute + CSS (preferred for many variants):**

```css
/* In your CSS file */
[data-variant="info"] { --alert-accent: var(--color-blue-500); }
[data-variant="success"] { --alert-accent: var(--color-green-500); }
[data-variant="warning"] { --alert-accent: var(--color-amber-500); }
```

```tsx
// Component uses the CSS variable, variant sets it via data attribute
const alertVariants = cva("border-(--alert-accent) bg-(--alert-accent)/10", {
  variants: {
    variant: {
      info: "text-blue-900",
      success: "text-green-900",
      warning: "text-amber-900",
    },
  },
})

export function Alert({ variant, className, ...props }: AlertProps) {
  return <div data-variant={variant} className={cn(alertVariants({ variant }), className)} {...props} />
}
```

**Option B: inline style (acceptable for dynamic CSS custom properties):**

```tsx
// When you need to SET a CSS variable dynamically, inline style is acceptable
const fillColors: Record<string, string> = {
  success: "var(--color-green-500)",
  danger: "var(--color-red-500)",
}

<div
  className="bg-(--fill-color) border-(--fill-color)/50"
  style={{ "--fill-color": fillColors[variant] } as React.CSSProperties}
/>
```

**Option C: use Tailwind color utilities directly (simplest, when you don't need CSS var indirection):**

```tsx
const variants = cva("base", {
  variants: {
    variant: {
      info: "border-blue-200 bg-blue-50 text-blue-900",
      error: "border-red-200 bg-red-50 text-red-900",
    },
  },
})
```

**WRONG — `theme()` in arbitrary properties does not work in v4:**
```tsx
"[--accent:theme(colors.blue.500)]"  // BREAKS — theme() removed for arbitrary values
```

### Renamed Utilities

**Defaults shifted down — the old "unlabeled" default is now `-sm`, and the old `-sm` is now `-xs`:**

| v3 (WRONG) | v4 (CORRECT) | Note |
|------------|--------------|------|
| `shadow` | `shadow-sm` | **Easy to miss — `shadow` looks valid but maps to a different size** |
| `shadow-sm` | `shadow-xs` | |
| `rounded` | `rounded-sm` | **Easy to miss — `rounded` looks valid but maps to a different size** |
| `rounded-sm` | `rounded-xs` | |
| `blur` | `blur-sm` | |
| `blur-sm` | `blur-xs` | |
| `drop-shadow` | `drop-shadow-sm` | |
| `drop-shadow-sm` | `drop-shadow-xs` | |
| `ring` | `ring-3` | Default ring width changed from 3px to 1px |
| `outline-none` | `outline-hidden` | `outline-none` now actually sets `outline-style: none` |

### Removed Utilities (No Direct Rename)

| v3 (REMOVED) | v4 Replacement |
|--------------|----------------|
| `ring-offset-*` | **Use `outline` + `outline-offset-*`** for ring-with-gap effect |
| `ring-offset-color-*` | **Use `outline-*` color utilities** |
| `bg-opacity-*` | Opacity modifiers: `bg-black/50` |
| `text-opacity-*` | Opacity modifiers: `text-black/50` |

**`ring-offset-*` is NOT `ring-inset`.** `ring-inset` was a v3 modifier for inset rings. In v4, inset rings/shadows have their own utilities (see below).

### Inset Shadows and Rings (New in v4)

```tsx
// Inset shadow — NOT "shadow-inset", it's a separate utility namespace
<div className="inset-shadow-sm inset-shadow-black/10">

// Inset ring — for inner borders
<div className="inset-ring inset-ring-black/10">
```

### Focus Ring Pattern (v4)

```tsx
// Ring with gap (replaces ring + ring-offset from v3):
<input className="focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--ring-color)" />

// Flush ring (no gap):
<input className="focus-visible:ring-2 focus-visible:ring-(--ring-color)" />

// Inset ring:
<input className="focus-visible:inset-ring-2 focus-visible:inset-ring-(--ring-color)" />
```

### Theme Configuration

```css
/* v4: CSS-first config with @theme directive */
@import "tailwindcss";

@theme inline {
  --color-primary: var(--primary);
  --radius-lg: var(--radius);
}
```

**v4 auto-generates CSS variables for all theme values.** Use these directly instead of `theme()`:

| Theme value | CSS variable pattern | Example |
|-------------|---------------------|---------|
| Colors | `--color-{name}-{shade}` | `var(--color-blue-500)` |
| Spacing | `--spacing-{value}` | `var(--spacing-4)` |
| Radii | `--radius-{size}` | `var(--radius-lg)` |
| Shadows | `--shadow-{size}` | `var(--shadow-sm)` |
| Breakpoints | `--breakpoint-{name}` | `var(--breakpoint-xl)` |

**`theme()` still exists** but ONLY for contexts where CSS variables don't work (media queries). Uses new syntax: `theme(--breakpoint-xl)` not `theme(screens.xl)`. **Never use `theme()` in arbitrary values or class strings.**

### Container Queries (Built-in)

```tsx
<div className="@container">
  <div className="@sm:flex @md:grid @lg:grid-cols-3">
```

### Dynamic Values (No Config Needed)

```tsx
<div className="mt-13 w-[347px] z-73">  // Works without defining in config
```

## Component Patterns

### Always Use `cn()` for Class Merging

```tsx
import { cn } from "@/lib/utils"

// CORRECT
<div className={cn("base-classes", active && "bg-active", className)}>

// WRONG — template literals cause Tailwind conflicts
<div className={`base-classes ${active ? 'bg-active' : ''}`}>
```

### Variants with `cva`

```tsx
import { cva, type VariantProps } from "class-variance-authority"

const cardVariants = cva(
  "rounded-sm border p-4 transition-colors",  // base (note: rounded-sm = old rounded in v4)
  {
    variants: {
      variant: {
        default: "border-border bg-card",
        destructive: "border-destructive/50 bg-destructive/10",
      },
      size: {
        sm: "p-2 text-sm",
        default: "p-4",
        lg: "p-6 text-lg",
      },
    },
    defaultVariants: { variant: "default", size: "default" },
  }
)

interface CardProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof cardVariants> {}

export function Card({ className, variant, size, ...props }: CardProps) {
  return <div className={cn(cardVariants({ variant, size, className }))} {...props} />
}
```

### Extending shadcn Components (Wrap, Don't Modify)

```tsx
import { Button, type ButtonProps } from "./button"

export function PrimaryButton({ className, ...props }: ButtonProps) {
  return <Button className={cn("bg-brand-500 hover:bg-brand-600", className)} {...props} />
}
```

**DO:** Customize via `className`, wrap to extend, use variant system.
**DON'T:** Modify shadcn source for one-off changes, override Radix accessibility props, use inline styles.

### Radix `asChild` / Slot Pattern

```tsx
import { Slot } from "@radix-ui/react-slot"

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  asChild?: boolean
}

function Button({ asChild = false, ...props }: ButtonProps) {
  const Comp = asChild ? Slot : "button"
  return <Comp {...props} />
}

// Renders as <a> with button styles
<Button asChild><a href="/home">Home</a></Button>
```

### Forms: react-hook-form + zod

```tsx
const schema = z.object({ email: z.string().email() })
type FormData = z.infer<typeof schema>

const form = useForm<FormData>({ resolver: zodResolver(schema) })
```

Use shadcn `<FormField>` / `<FormItem>` / `<FormControl>` / `<FormMessage>` for consistent field rendering.

### Loading & Error States

```tsx
// Component-level
if (isLoading) return <Skeleton className="h-32 w-full" />
if (error) return <Alert variant="destructive"><AlertDescription>{error.message}</AlertDescription></Alert>

// Button loading
<Button disabled={isLoading}>
  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
  {isLoading ? "Saving..." : "Save"}
</Button>
```

## File Organization

```
components/
  ui/           # shadcn/ui base components
  forms/        # Form-specific components
  layout/       # Header, Sidebar, etc.
  [feature]/    # Feature-specific components
```

## Accessibility Checklist

- Interactive elements keyboard accessible
- Visible focus states (`focus-visible:outline-2 focus-visible:outline-offset-2`)
- Color contrast WCAG AA (4.5:1)
- Form fields have labels
- Error messages announced to screen readers
- Loading states use `aria-busy`
- Decorative icons have `aria-hidden="true"`

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| `bg-[--brand]` or `bg-[var(--brand)]` | `bg-(--brand)` — v4 parentheses |
| `shadow-sm` for subtle shadow | `shadow-xs` — sm shifted to xs in v4 |
| `shadow` for default shadow | `shadow-sm` — defaults shifted down |
| `rounded-sm` for subtle rounding | `rounded-xs` — sm shifted to xs in v4 |
| `rounded` for default rounding | `rounded-sm` — defaults shifted down |
| `ring` for 3px ring | `ring-3` — default ring width is now 1px |
| `ring-offset-2` for ring with gap | `outline-2 outline-offset-2` — ring-offset removed |
| `shadow-inset` for inner shadow | `inset-shadow-sm` — separate utility namespace in v4 |
| `outline-none` to hide outline | `outline-hidden` — `outline-none` now sets `outline-style: none` |
| `` className={`base ${x}`} `` | `className={cn("base", x)}` |
| `style={{ color: "var(--x)" }}` for reading | `className="text-(--x)"` |
| `[--var:theme(colors.x.y)]` to set CSS var | Use data-attribute + CSS or inline `style` with `var(--color-x-y)` |
| `theme(colors.blue.500)` in arbitrary values | Use `var(--color-blue-500)` directly |
| Modifying shadcn source for one-off | Wrap the component instead |
| Overriding Radix `aria-*` props | Don't — Radix handles accessibility |

## Red Flags — You're Using v3 Syntax

**STOP and fix if you see any of these in your output:**

- Square brackets around CSS variables: `[--anything]` → use `(--anything)`
- `theme()` in arbitrary values or class strings → use `var(--color-*)` directly
- `[--var:theme(colors.x.y)]` — doubly wrong: brackets AND theme()
- `shadow-sm` / `rounded-sm` / `blur-sm` when you mean the SMALLEST size → use `-xs`
- `shadow` / `rounded` / `blur` as bare utilities → they shifted down, now use `-sm`
- `ring-offset-*` utilities → removed; use `outline-*` + `outline-offset-*`
- `shadow-inset` → use `inset-shadow-*` (separate namespace in v4)
- `ring` expecting 3px width → use `ring-3` (default is now 1px)
- `outline-none` for hiding outlines → use `outline-hidden`
- Inline `style={{ }}` for READING a CSS variable → use Tailwind `(--var)` syntax
- Template literals for className: `` `base ${x}` `` → use `cn()`

**Inline `style` IS acceptable for SETTING CSS custom properties** (e.g., `style={{ "--fill": "var(--color-blue-500)" }}`). It is NOT acceptable for reading them when Tailwind can express it.

**These apply inside cva() strings too, not just JSX className props.**
