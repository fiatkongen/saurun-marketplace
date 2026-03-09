// =============================================================================
// ProductCatalogPage — Main page composing all catalog pieces
// URL params drive filter state. Zustand for cart. TanStack Query for data.
// =============================================================================

import { cn } from "./cn"
import { Header } from "./Header"
import { SearchBar } from "./SearchBar"
import { FilterSidebar } from "./FilterSidebar"
import { ViewToggle } from "./ViewToggle"
import { SortSelect } from "./SortSelect"
import { ProductGrid } from "./ProductGrid"
import { Pagination } from "./Pagination"
import { useFilterParams } from "./useFilterParams"
import { useProducts } from "./useProducts"

export function ProductCatalogPage() {
  const { filters, setFilters } = useFilterParams()
  const { data, isPending, isFetching } = useProducts(filters)

  const products = data?.items ?? []
  const totalPages = data?.totalPages ?? 1
  const totalItems = data?.totalItems ?? 0

  return (
    <div className="min-h-screen bg-stone-50">
      <Header />

      <main className="max-w-7xl mx-auto px-6 py-8">
        {/* Page Header */}
        <div className="mb-8">
          <h1
            data-testid="catalog-page-title"
            className="text-3xl font-bold tracking-tight text-stone-900 mb-1"
          >
            Products
          </h1>
          <p className="text-sm text-stone-400">
            {isPending
              ? "Loading products..."
              : `${totalItems} product${totalItems !== 1 ? "s" : ""} found`}
          </p>
        </div>

        {/* Search Bar */}
        <SearchBar
          value={filters.search}
          onChange={(search) => setFilters({ search })}
          className="mb-6 max-w-xl"
        />

        {/* Main Layout: Sidebar + Content */}
        <div className="flex gap-8">
          {/* Filters Sidebar */}
          <FilterSidebar
            filters={filters}
            onFilterChange={setFilters}
            className="hidden lg:block"
          />

          {/* Product Content */}
          <div className="flex-1 min-w-0">
            {/* Toolbar: Sort + View Toggle + Result Count */}
            <div
              data-testid="catalog-toolbar"
              className={cn(
                "flex items-center justify-between gap-4 mb-6",
                "pb-4 border-b border-stone-200/60"
              )}
            >
              <div className="flex items-center gap-3">
                <SortSelect
                  value={filters.sortBy}
                  onChange={(sortBy) => setFilters({ sortBy })}
                />
                {isFetching && !isPending && (
                  <div
                    className="w-4 h-4 rounded-full border-2 border-amber-500 border-t-transparent animate-spin"
                    aria-label="Updating results"
                  />
                )}
              </div>
              <ViewToggle
                view={filters.view}
                onChange={(view) => setFilters({ view })}
              />
            </div>

            {/* Active Filters Pills */}
            <ActiveFilterPills filters={filters} onClear={setFilters} />

            {/* Product Grid/List */}
            <ProductGrid
              products={products}
              view={filters.view}
              isLoading={isPending}
            />

            {/* Pagination */}
            <Pagination
              currentPage={filters.page}
              totalPages={totalPages}
              onPageChange={(page) => setFilters({ page })}
              className="mt-10"
            />
          </div>
        </div>
      </main>
    </div>
  )
}

// ============================================
// Active Filter Pills
// ============================================

function ActiveFilterPills({
  filters,
  onClear,
}: {
  filters: ReturnType<typeof useFilterParams>["filters"]
  onClear: ReturnType<typeof useFilterParams>["setFilters"]
}) {
  const pills: { label: string; onRemove: () => void }[] = []

  if (filters.search) {
    pills.push({
      label: `Search: "${filters.search}"`,
      onRemove: () => onClear({ search: "" }),
    })
  }

  for (const cat of filters.categories) {
    pills.push({
      label: cat,
      onRemove: () =>
        onClear({
          categories: filters.categories.filter((c) => c !== cat),
        }),
    })
  }

  if (filters.priceMin > 0 || filters.priceMax < 1000) {
    pills.push({
      label: `$${filters.priceMin} - $${filters.priceMax}`,
      onRemove: () => onClear({ priceMin: 0, priceMax: 1000 }),
    })
  }

  if (filters.minRating > 0) {
    pills.push({
      label: `${filters.minRating}+ stars`,
      onRemove: () => onClear({ minRating: 0 }),
    })
  }

  if (filters.inStockOnly) {
    pills.push({
      label: "In stock only",
      onRemove: () => onClear({ inStockOnly: false }),
    })
  }

  if (pills.length === 0) return null

  return (
    <div
      data-testid="catalog-active-filters"
      className="flex flex-wrap items-center gap-2 mb-5"
    >
      {pills.map((pill) => (
        <span
          key={pill.label}
          className={cn(
            "inline-flex items-center gap-1.5 px-3 py-1 rounded-full",
            "bg-stone-100 text-xs font-medium text-stone-600",
            "transition-colors hover:bg-stone-200"
          )}
        >
          {pill.label}
          <button
            data-testid={`filter-pill-remove-${pill.label}`}
            type="button"
            onClick={pill.onRemove}
            className="text-stone-400 hover:text-stone-600"
            aria-label={`Remove filter: ${pill.label}`}
          >
            <svg className="w-3.5 h-3.5" viewBox="0 0 20 20" fill="currentColor">
              <path d="M6.28 5.22a.75.75 0 00-1.06 1.06L8.94 10l-3.72 3.72a.75.75 0 101.06 1.06L10 11.06l3.72 3.72a.75.75 0 101.06-1.06L11.06 10l3.72-3.72a.75.75 0 00-1.06-1.06L10 8.94 6.28 5.22z" />
            </svg>
          </button>
        </span>
      ))}
    </div>
  )
}
