Here's the implementation:

**CSS file (add to your global styles or component CSS):**

```css
/* Alert variant accent colors */
[data-variant="info"] {
  --alert-accent: var(--color-blue-500);
  --alert-bg: var(--color-blue-50);
  --alert-text: var(--color-blue-900);
  --alert-border: var(--color-blue-200);
}

[data-variant="success"] {
  --alert-accent: var(--color-green-500);
  --alert-bg: var(--color-green-50);
  --alert-text: var(--color-green-900);
  --alert-border: var(--color-green-200);
}

[data-variant="warning"] {
  --alert-accent: var(--color-amber-500);
  --alert-bg: var(--color-amber-50);
  --alert-text: var(--color-amber-900);
  --alert-border: var(--color-amber-200);
}

[data-variant="error"] {
  --alert-accent: var(--color-red-500);
  --alert-bg: var(--color-red-50);
  --alert-text: var(--color-red-900);
  --alert-border: var(--color-red-200);
}
```

**Component file:**

```tsx
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const alertVariants = cva(
  [
    "relative rounded-sm border border-(--alert-border) bg-(--alert-bg) p-4 shadow-xs",
    "text-(--alert-text)",
    "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--alert-accent)",
  ].join(" "),
  {
    variants: {
      variant: {
        info: "",
        success: "",
        warning: "",
        error: "",
      },
    },
    defaultVariants: { variant: "info" },
  }
)

interface AlertProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof alertVariants> {
  icon?: React.ReactNode
}

export function Alert({ className, variant, icon, children, ...props }: AlertProps) {
  return (
    <div
      role="alert"
      tabIndex={-1}
      data-variant={variant}
      className={cn(alertVariants({ variant }), className)}
      {...props}
    >
      <div className="flex items-start gap-3">
        {icon && (
          <span className="mt-0.5 shrink-0 text-(--alert-accent)">{icon}</span>
        )}
        <div className="flex-1">{children}</div>
      </div>
      <div className="absolute inset-y-0 left-0 w-1 rounded-l-sm bg-(--alert-accent)" />
    </div>
  )
}
```

Key decisions and why:

- **`data-variant` + CSS pattern**: The CSS file maps each variant to a set of CSS variables (`--alert-accent`, `--alert-bg`, etc.). The component's `cva` base classes read those variables with parentheses syntax (`bg-(--alert-bg)`). The `cva` variant strings are empty because the data attribute does all the styling work. This keeps the component clean and makes adding new variants a CSS-only change.

- **`shadow-xs`**: Smallest shadow size per the Tailwind v4 scale (`-xs` < `-sm` < default).

- **`focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--alert-accent)`**: Ring-with-gap pattern uses `outline` + `outline-offset`, not `ring-offset`. The accent color is read from the CSS variable so focus rings match the variant color.

- **Parentheses syntax throughout**: `bg-(--alert-bg)`, `text-(--alert-text)`, `border-(--alert-border)` -- never bracket syntax `[--var]` or `[var(--var)]`.

- **Left accent bar**: The `absolute` positioned `div` with `bg-(--alert-accent)` gives a colored left edge that automatically matches the variant, no extra variant logic needed.

- **`cn()` for class merging**: Allows consumers to override classes via `className` prop without template literal conflicts.
