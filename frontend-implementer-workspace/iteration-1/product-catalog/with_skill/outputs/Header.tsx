// =============================================================================
// Header — Top bar with logo placeholder and cart icon with item count badge
// =============================================================================

import { cn } from "./cn"
import { useCartCount } from "./useCartStore"

interface HeaderProps {
  className?: string
}

export function Header({ className }: HeaderProps) {
  const itemCount = useCartCount()

  return (
    <header
      data-testid="catalog-header"
      className={cn(
        "sticky top-0 z-40 w-full",
        "bg-white/80 backdrop-blur-sm border-b border-stone-200/60",
        className
      )}
    >
      <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
        {/* Logo / Brand */}
        <div className="flex items-center gap-3">
          <div
            data-asset="icon-brand-logo"
            className="w-8 h-8 rounded-lg bg-stone-900"
          />
          <span className="text-lg font-bold tracking-tight text-stone-900">
            Catalog
          </span>
        </div>

        {/* Cart Button */}
        <button
          data-testid="catalog-header-cart"
          type="button"
          className={cn(
            "relative p-2.5 rounded-lg",
            "text-stone-600 transition-colors",
            "hover:bg-stone-100 hover:text-stone-900"
          )}
          aria-label={`Shopping cart with ${itemCount} items`}
        >
          <svg
            className="w-5.5 h-5.5"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={1.8}
            strokeLinecap="round"
            strokeLinejoin="round"
            aria-hidden="true"
          >
            <path d="M6 2L3 6v14a2 2 0 002 2h14a2 2 0 002-2V6l-3-4z" />
            <line x1="3" y1="6" x2="21" y2="6" />
            <path d="M16 10a4 4 0 01-8 0" />
          </svg>

          {/* Badge */}
          {itemCount > 0 && (
            <span
              data-testid="catalog-header-cart-badge"
              className={cn(
                "absolute -top-0.5 -right-0.5",
                "flex items-center justify-center",
                "min-w-5 h-5 px-1 rounded-full",
                "bg-amber-600 text-white text-[11px] font-bold tabular-nums",
                "animate-in zoom-in-50 duration-200"
              )}
            >
              {itemCount > 99 ? "99+" : itemCount}
            </span>
          )}
        </button>
      </div>
    </header>
  )
}
