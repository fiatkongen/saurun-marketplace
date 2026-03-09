// =============================================================================
// ProductGrid — Renders products in grid or list layout with loading/empty states
// =============================================================================

import { cn } from "./cn"
import { ProductCard } from "./ProductCard"
import type { Product } from "./types"

interface ProductGridProps {
  products: Product[]
  view: "grid" | "list"
  isLoading: boolean
  className?: string
}

// ============================================
// Skeleton Loader
// ============================================

function ProductSkeleton({ view }: { view: "grid" | "list" }) {
  if (view === "list") {
    return (
      <div className="flex gap-5 p-4 rounded-xl border border-stone-200/80 animate-pulse">
        <div className="w-40 aspect-square rounded-lg bg-stone-200" />
        <div className="flex-1 space-y-3 py-1">
          <div className="h-3 w-20 rounded bg-stone-200" />
          <div className="h-4 w-3/4 rounded bg-stone-200" />
          <div className="h-3 w-full rounded bg-stone-100" />
          <div className="h-3 w-2/3 rounded bg-stone-100" />
        </div>
        <div className="w-24 space-y-3 py-1">
          <div className="h-5 w-full rounded bg-stone-200" />
          <div className="h-3 w-16 rounded bg-stone-100" />
        </div>
      </div>
    )
  }

  return (
    <div className="rounded-xl border border-stone-200/80 animate-pulse">
      <div className="aspect-[4/3] rounded-t-xl bg-stone-200" />
      <div className="p-4 space-y-2.5">
        <div className="h-3 w-16 rounded bg-stone-200" />
        <div className="h-4 w-3/4 rounded bg-stone-200" />
        <div className="h-3 w-24 rounded bg-stone-100" />
        <div className="flex justify-between items-center pt-1">
          <div className="h-5 w-16 rounded bg-stone-200" />
          <div className="h-3 w-12 rounded bg-stone-100" />
        </div>
      </div>
    </div>
  )
}

// ============================================
// Empty State
// ============================================

function EmptyState() {
  return (
    <div
      data-testid="catalog-empty-state"
      className="flex flex-col items-center justify-center py-20 text-center"
    >
      <div
        data-asset="illustration-empty-catalog"
        className="w-32 h-32 rounded-full bg-stone-100 mb-6"
      />
      <h3 className="text-lg font-semibold text-stone-700 mb-1">
        No products found
      </h3>
      <p className="text-sm text-stone-400 max-w-xs">
        Try adjusting your search or filters to find what you are looking for.
      </p>
    </div>
  )
}

// ============================================
// Main Grid Component
// ============================================

export function ProductGrid({
  products,
  view,
  isLoading,
  className,
}: ProductGridProps) {
  if (isLoading && products.length === 0) {
    return (
      <div
        data-testid="catalog-loading"
        className={cn(
          view === "grid"
            ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5"
            : "flex flex-col gap-4",
          className
        )}
      >
        {Array.from({ length: 6 }, (_, i) => (
          <ProductSkeleton key={i} view={view} />
        ))}
      </div>
    )
  }

  if (products.length === 0) {
    return <EmptyState />
  }

  return (
    <div
      data-testid="catalog-product-grid"
      className={cn(
        view === "grid"
          ? "grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5"
          : "flex flex-col gap-4",
        isLoading && "opacity-60 pointer-events-none transition-opacity duration-300",
        className
      )}
    >
      {products.map((product) => (
        <ProductCard key={product.id} product={product} view={view} />
      ))}
    </div>
  )
}
