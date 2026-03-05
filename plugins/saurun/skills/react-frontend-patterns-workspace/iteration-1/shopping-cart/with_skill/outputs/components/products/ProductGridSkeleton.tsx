/**
 * ProductGridSkeleton
 *
 * Loading state for the product grid.
 * Shown while TanStack Query fetches are pending.
 */

export function ProductGridSkeleton() {
  return (
    <div>
      {/* Category filter skeleton */}
      <div className="mb-6 flex gap-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <div
            key={i}
            className="h-8 w-20 animate-pulse rounded-full bg-muted"
          />
        ))}
      </div>

      {/* Product cards skeleton */}
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        {Array.from({ length: 8 }).map((_, i) => (
          <div
            key={i}
            className="flex flex-col overflow-hidden rounded-lg border bg-card"
          >
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
        ))}
      </div>
    </div>
  )
}
