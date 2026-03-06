---
name: react-enforcement
description: Use when building React frontends with Zustand, TanStack Query, Tailwind v4, or shadcn/ui. Mandatory constraints for component architecture, state management, styling, and pre-commit verification.
---

# React Frontend — Enforcement Protocol

Mandatory constraints for all React frontend code. Violations must be fixed before committing. These rules override implementation plan structure when they conflict.

## Mandatory Rules

### 1. Zustand: Selector-Only Access

Always use selectors. Bare `useStore()` is a violation.

```tsx
// CORRECT
const items = useCartStore((s) => s.items)
const { items, total } = useCartStore(useShallow((s) => ({ items: s.items, total: s.total })))

// VIOLATION — causes re-render on ANY state change
const store = useCartStore()
const { items } = useCartStore()
```

**Export selector hooks** from every store file:

```tsx
export const useCartItems = () => useCartStore((s) => s.items)
export const useCartTotal = () => useCartStore((s) => s.items.reduce((sum, i) => sum + i.price * i.quantity, 0))
export const useCartActions = () => useCartStore(useShallow((s) => ({ addItem: s.addItem, removeItem: s.removeItem })))
```

A store without exported selector hooks is a violation. Derived state stored in the store (instead of computed in selector) is a violation.

### 2. State Colocation

| Data type | Where it lives | Violation |
|-----------|---------------|-----------|
| Server data | TanStack Query cache | `useState` + `fetch`, Zustand for API data |
| Global UI | Zustand store | prop drilling through 3+ levels |
| URL state | Router params/search | Zustand for filters that belong in URL |
| Form input | react-hook-form | manual `useState` per field |
| Local | `useState` | Zustand for modal open/hover |

`useState` + `useEffect` + `fetch` is always a violation. Use TanStack Query.

### 3. TanStack Query Patterns

Custom hooks wrap `useQuery`/`useMutation`. Direct usage in components is a violation.

```tsx
// Query key factory (one per domain)
export const buildKeys = {
  all: ['builds'] as const,
  lists: () => [...buildKeys.all, 'list'] as const,
  list: (filters: BuildFilters) => [...buildKeys.lists(), filters] as const,
  detail: (id: number) => [...buildKeys.all, 'detail', id] as const,
}

// Custom hook
export function useBuilds(filters?: BuildFilters) {
  return useQuery({
    queryKey: buildKeys.list(filters ?? {}),
    queryFn: () => api.builds.list(filters),
  })
}

// Mutation with invalidation
export function useCreateBuild() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.builds.create,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: buildKeys.lists() }),
  })
}
```

A mutation without `onSuccess` invalidation is a violation.

### 4. Tailwind v4 Syntax

| Correct | Violation | Why |
|---------|-----------|-----|
| `bg-(--brand)` | `bg-[--brand]` or `bg-[var(--brand)]` | Parentheses for CSS vars |
| `cn("base", active && "bg-active")` | `` className={`base ${x}`} `` | cn() for class merging |
| `text-red-500!` | `!text-red-500` | Important is suffix |
| `shadow-xs`, `rounded-xs` | `shadow-sm` as smallest | `-xs` is smallest |
| `inset-shadow-sm` | `shadow-inset` | Separate namespace |
| `outline-hidden` | `outline-none` | `outline-none` sets style:none |
| `outline-2 outline-offset-2` | `ring-offset-2` | Use outline for ring-with-gap |
| `var(--color-blue-500)` | `theme(colors.blue.500)` | CSS variables directly |

Template literal in `className` is always a violation. Use `cn()` from `@/lib/utils`.

### 5. Component Patterns

**cva() for variants:**

```tsx
const buttonVariants = cva("rounded-sm px-4 py-2 font-medium transition-colors", {
  variants: {
    variant: {
      default: "bg-primary text-primary-foreground hover:bg-primary/90",
      destructive: "bg-destructive text-destructive-foreground hover:bg-destructive/90",
    },
    size: { sm: "px-2 py-1 text-sm", default: "px-4 py-2", lg: "px-6 py-3 text-lg" },
  },
  defaultVariants: { variant: "default", size: "default" },
})

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement>, VariantProps<typeof buttonVariants> {}
export function Button({ className, variant, size, ...props }: ButtonProps) {
  return <button className={cn(buttonVariants({ variant, size, className }))} {...props} />
}
```

**Wrap shadcn, don't modify source.** Modifying shadcn source files is a violation.

**Error boundaries:** Every app needs page-level + feature-level error boundaries. No error boundaries = violation.

### 6. File Organization

```
src/
  components/
    ui/              # shadcn/ui primitives (don't modify)
    [feature]/       # Feature components
      FeatureCard.tsx
      __tests__/
        FeatureCard.test.tsx
    layout/          # Shell, nav, sidebar
  stores/            # Zustand stores (one per domain)
    useCartStore.ts
  hooks/             # TanStack Query hooks
    useProducts.ts
  pages/             # Route components
    ProductsPage.tsx
  lib/
    utils.ts         # cn() helper
    api.ts           # API client
  types/
    api.ts           # Shared API types
```

Tests in `__tests__/` subfolder next to components. Query hooks in `hooks/`. Stores in `stores/`. Mixing these locations is a violation.

### 7. Accessibility Baseline

- All interactive elements: `focus-visible:outline-2 focus-visible:outline-offset-2`
- Form fields have associated `<label>` elements
- Decorative icons: `aria-hidden="true"`
- Loading states: `aria-busy="true"` on container
- Color contrast: WCAG AA (4.5:1 for text)

Interactive element without visible focus state is a violation.

### 8. data-testid Convention

All interactive elements MUST have `data-testid`. Convention: `{component}-{element}-{action?}`

```tsx
<button data-testid="recipe-form-submit">Save</button>
<input data-testid="recipe-form-title" />
<div data-testid="recipe-card-123">...</div>  // Include ID for list items
```

Interactive element without `data-testid` is a violation.

## Pre-Commit Verification

**Run before committing. If ANY fails, fix it.**

```bash
# 1. No bare useStore() calls (destructuring from bare store)
grep -rn "= use.*Store()" src/ --include="*.tsx" --include="*.ts" | grep -v "(s)" | grep -v "useShallow" | grep -v "getState" | grep -v "node_modules" && echo "FAIL: bare useStore()"

# 2. No Tailwind v3 CSS variable syntax
grep -rn '\[--' src/ --include="*.tsx" --include="*.css" | grep -v "node_modules" && echo "FAIL: v3 CSS var syntax [--var]"

# 3. No template literal classNames
grep -rn 'className={`' src/ --include="*.tsx" | grep -v "node_modules" && echo "FAIL: template literal className"

# 4. cn() utility exists
grep -rn "export function cn" src/lib/utils.ts 2>/dev/null | grep -q "." || echo "FAIL: missing cn() utility"

# 5. No vi.mock on stores, components, or hooks
grep -rn "vi.mock" src/ --include="*.test.*" | grep -iv "msw\|server\|window\|navigator\|IntersectionObserver\|ResizeObserver\|location" && echo "FAIL: vi.mock on non-boundary"

# 6. Stores export selector hooks
for f in src/stores/*.ts; do [ -f "$f" ] && grep -qL "export.*const.*use.*=" "$f" && echo "FAIL: $f missing selector hooks"; done

# 7. TanStack Query hooks exist in hooks/
ls src/hooks/use*.ts 2>/dev/null | grep -q "." || echo "FAIL: no query hooks in hooks/"

# 8. No useState+useEffect fetch pattern
grep -B2 -A5 "useEffect" src/ -r --include="*.tsx" | grep -q "fetch\|axios\|\.get\|\.post" && echo "FAIL: fetch in useEffect"

# 9. Tests have assertions
find src/ -name "*.test.*" -exec grep -L "expect(" {} \; 2>/dev/null | head -5 | while read f; do echo "FAIL: $f has no assertions"; done

# 10. data-testid present in components
grep -rn "data-testid" src/ --include="*.tsx" | grep -v "node_modules" | grep -v "test" | head -1 > /dev/null || echo "FAIL: no data-testid in components"
```
