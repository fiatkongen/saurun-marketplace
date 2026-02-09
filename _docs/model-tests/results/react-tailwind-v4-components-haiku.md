## Alert Component Implementation

**CSS file:**

```css
/* Alert variant CSS variables mapped to Tailwind color scales */

[data-variant="info"] {
  --alert-accent: var(--color-blue-500);
}

[data-variant="success"] {
  --alert-accent: var(--color-green-500);
}

[data-variant="warning"] {
  --alert-accent: var(--color-amber-500);
}

[data-variant="error"] {
  --alert-accent: var(--color-red-500);
}
```

**Component file:**

```tsx
import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const alertVariants = cva(
  "relative rounded-sm border p-4 transition-colors shadow-xs",
  {
    variants: {
      variant: {
        info: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-blue-900",
        success: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-green-900",
        warning: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-amber-900",
        error: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-red-900",
      },
    },
    defaultVariants: { variant: "info" },
  }
)

interface AlertProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof alertVariants> {}

const Alert = React.forwardRef<HTMLDivElement, AlertProps>(
  ({ className, variant, ...props }, ref) => (
    <div
      ref={ref}
      data-variant={variant}
      className={cn(alertVariants({ variant }), className)}
      {...props}
    />
  )
)
Alert.displayName = "Alert"

const AlertTitle = React.forwardRef<
  HTMLParagraphElement,
  React.HTMLAttributes<HTMLHeadingElement>
>(({ className, ...props }, ref) => (
  <h5
    ref={ref}
    className={cn("mb-1 font-semibold leading-none tracking-tight", className)}
    {...props}
  />
))
AlertTitle.displayName = "AlertTitle"

const AlertDescription = React.forwardRef<
  HTMLParagraphElement,
  React.HTMLAttributes<HTMLParagraphElement>
>(({ className, ...props }, ref) => (
  <div ref={ref} className={cn("text-sm [&_p]:leading-relaxed", className)} {...props} />
))
AlertDescription.displayName = "AlertDescription"

export { Alert, AlertTitle, AlertDescription, alertVariants }
export type { AlertProps }
```

**Interactive variant with focus states:**

```tsx
const interactiveAlertVariants = cva(
  "relative rounded-sm border p-4 transition-colors shadow-xs cursor-pointer focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--alert-accent)",
  {
    variants: {
      variant: {
        info: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-blue-900 hover:bg-(--alert-accent)/15",
        success: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-green-900 hover:bg-(--alert-accent)/15",
        warning: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-amber-900 hover:bg-(--alert-accent)/15",
        error: "border-(--alert-accent)/30 bg-(--alert-accent)/10 text-red-900 hover:bg-(--alert-accent)/15",
      },
    },
    defaultVariants: { variant: "info" },
  }
)
```

Key implementation details:
- CSS variable syntax uses parentheses: `bg-(--alert-accent)` not `bg-[--alert-accent]`
- Data-attribute pattern: `data-variant={variant}` with CSS `[data-variant="info"]` selectors
- Shadow: `shadow-xs` for smallest size
- Focus: `focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-(--alert-accent)`
- `cn()` for class merging, `cva()` for type-safe variants
- `var(--color-blue-500)` for theme color references in CSS
