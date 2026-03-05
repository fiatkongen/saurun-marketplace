/**
 * ProductGrid
 *
 * Fetches products via TanStack Query hook and renders a filtered grid.
 * Category filter state is managed locally (could be lifted to URL params).
 * Server data lives in query cache -- never Zustand.
 */

import { useState, useMemo } from 'react'
import { useProducts } from '../../hooks/useProducts'
import { ProductCard } from './ProductCard'
import { CategoryFilter } from './CategoryFilter'
import { ProductGridSkeleton } from './ProductGridSkeleton'

export function ProductGrid() {
  const [selectedCategory, setSelectedCategory] = useState<string | undefined>(
    undefined
  )

  const { data: products, isPending, error } = useProducts(selectedCategory)

  // Derive unique categories from all products (fetch unfiltered for the list)
  const { data: allProducts } = useProducts()
  const categories = useMemo(() => {
    if (!allProducts) return []
    return [...new Set(allProducts.map((p) => p.category))].sort()
  }, [allProducts])

  if (isPending) return <ProductGridSkeleton />

  if (error) {
    return (
      <div
        data-testid="product-grid-error"
        className="rounded-md border border-destructive/50 bg-destructive/10 p-6 text-center"
      >
        <p className="text-destructive">{error.message}</p>
      </div>
    )
  }

  return (
    <div>
      <CategoryFilter
        categories={categories}
        selected={selectedCategory}
        onSelect={setSelectedCategory}
      />

      {products.length === 0 ? (
        <p
          data-testid="product-grid-empty"
          className="py-12 text-center text-muted-foreground"
        >
          No products found.
        </p>
      ) : (
        <div
          data-testid="product-grid"
          className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        >
          {products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  )
}

// ============================================
// Loading Skeleton
// ============================================

function ProductCardSkeleton() {
  return (
    <div className="flex flex-col overflow-hidden rounded-lg border bg-card">
      <div className="aspect-square animate-pulse bg-muted" />
      <div className="flex flex-col gap-2 p-4">
        <div className="h-3 w-16 animate-pulse rounded bg-muted" />
        <div className="h-5 w-3/4 animate-pulse rounded bg-muted" />
        <div className="h-4 w-full animate-pulse rounded bg-muted" />
        <div className="mt-3 flex items-center justify-between">
          <div className="h-6 w-16 animate-pulse rounded bg-muted" />
          <div className="h-8 w-24 animate-pulse rounded bg-muted" />
        </div>
      </div>
    </div>
  )
}
