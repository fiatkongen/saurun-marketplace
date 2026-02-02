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

### Setting CSS Variables in Tailwind v4

**NEVER use `theme()` function.** It does not exist in v4.

```tsx
// v4 CORRECT — use actual color values or CSS variables defined in @theme
const variants = cva("base", {
  variants: {
    variant: {
      info: "border-blue-200 bg-blue-50 text-blue-900",
      error: "border-red-200 bg-red-50 text-red-900",
    },
  },
})

// v3 WRONG — theme() function does not exist in v4
"[--accent:theme(colors.blue.500)]"  // BREAKS

// If you need a CSS custom property per variant, set it in CSS or use a data attribute:
// CSS: [data-variant="info"] { --notification-accent: var(--color-blue-500); }
// TSX: <div data-variant={variant} className="text-(--notification-accent)">
```

### Renamed Utilities

| v3 (WRONG) | v4 (CORRECT) |
|------------|--------------|
| `shadow-sm` | `shadow-xs` |
| `shadow` | `shadow-sm` |
| `rounded-sm` | `rounded-xs` |
| `rounded` | `rounded-sm` |
| `blur-sm` | `blur-xs` |
| `ring-offset-*` | `ring-inset` |

**Every `sm` shifted down to `xs`. The old `sm` name now maps to a LARGER size.**

### Theme Configuration

```css
/* v4: CSS-first config with @theme directive */
@import "tailwindcss";

@theme inline {
  --color-primary: var(--primary);
  --radius-lg: var(--radius);
}
```

No `theme()` function in arbitrary values. Use CSS variables directly.

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
- Visible focus states (`focus-visible:ring-2`)
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
| `rounded-sm` for subtle rounding | `rounded-xs` — sm shifted to xs in v4 |
| `ring-offset-2` | Use `ring-inset` or remove — `ring-offset-*` does not exist in v4 |
| `` className={`base ${x}`} `` | `className={cn("base", x)}` |
| `style={{ color: "var(--x)" }}` | `className="text-(--x)"` |
| `theme(colors.blue.500)` in arbitrary values | Use CSS variables via `@theme` |
| Modifying shadcn source for one-off | Wrap the component instead |
| Overriding Radix `aria-*` props | Don't — Radix handles accessibility |

## Red Flags — You're Using v3 Syntax

**STOP and fix if you see any of these in your output:**

- Square brackets around CSS variables: `[--anything]` → use `(--anything)`
- `theme()` function ANYWHERE — does not exist in v4
- `[--var:theme(colors.x.y)]` — doubly wrong: brackets AND theme()
- `shadow-sm` / `rounded-sm` / `blur-sm` when you mean the SMALLEST size → use `-xs`
- `ring-offset-*` utilities → use `ring-inset` or remove
- Inline `style={{ }}` for something Tailwind can express
- Template literals for className: `` `base ${x}` `` → use `cn()`

**These apply inside cva() strings too, not just JSX className props.**
