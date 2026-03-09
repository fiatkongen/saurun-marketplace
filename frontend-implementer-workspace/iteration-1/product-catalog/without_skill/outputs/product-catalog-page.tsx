// ============================================================================
// ProductCatalogPage — Main page composing all catalog components
// ============================================================================

import { useMemo } from "react";
import { cn } from "@/lib/utils";
import { useCatalogParams } from "./use-catalog-params";
import { useFilteredProducts } from "./use-filtered-products";
import { Header } from "./header";
import { SearchBar } from "./search-bar";
import { FilterSidebar } from "./filter-sidebar";
import { MobileFilterDrawer } from "./mobile-filter-drawer";
import { ViewToggle } from "./view-toggle";
import { ProductCard } from "./product-card";
import { Pagination } from "./pagination";
import { PRICE_MIN, PRICE_MAX } from "./data";

export function ProductCatalogPage() {
  const {
    filters,
    pagination,
    viewMode,
    setFilters,
    setViewMode,
    setPage,
    setPageSize,
  } = useCatalogParams();

  const { products, totalCount, totalPages } = useFilteredProducts(
    filters,
    pagination
  );

  const activeFilterCount = useMemo(() => {
    let count = 0;
    if (filters.categories.length > 0) count++;
    if (filters.priceRange[0] !== PRICE_MIN || filters.priceRange[1] !== PRICE_MAX) count++;
    if (filters.minRating > 0) count++;
    if (filters.inStockOnly) count++;
    return count;
  }, [filters]);

  return (
    <div className="min-h-screen bg-background text-foreground">
      <Header />

      <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        {/* Top bar: search + view toggle + mobile filter trigger */}
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between mb-6">
          <div className="flex-1 max-w-md">
            <SearchBar
              value={filters.search}
              onChange={(search) => setFilters({ search })}
            />
          </div>

          <div className="flex items-center gap-2">
            <MobileFilterDrawer
              filters={filters}
              onFilterChange={setFilters}
              activeFilterCount={activeFilterCount}
            />
            <ViewToggle value={viewMode} onChange={setViewMode} />
          </div>
        </div>

        {/* Content: sidebar + product grid */}
        <div className="flex gap-8">
          {/* Desktop sidebar */}
          <div className="hidden lg:block">
            <FilterSidebar filters={filters} onFilterChange={setFilters} />
          </div>

          {/* Products area */}
          <div className="flex-1 min-w-0">
            {/* Result count */}
            <p className="text-sm text-muted-foreground mb-4">
              {totalCount === 0
                ? "No products found"
                : `Showing ${totalCount} product${totalCount !== 1 ? "s" : ""}`}
              {filters.search && (
                <span>
                  {" "}
                  for &ldquo;
                  <span className="font-medium text-foreground">
                    {filters.search}
                  </span>
                  &rdquo;
                </span>
              )}
            </p>

            {/* Product grid / list */}
            {products.length > 0 ? (
              <div
                className={cn(
                  viewMode === "grid"
                    ? "grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4"
                    : "flex flex-col gap-3"
                )}
              >
                {products.map((product) => (
                  <ProductCard
                    key={product.id}
                    product={product}
                    viewMode={viewMode}
                  />
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center py-20 text-center">
                <p className="text-lg font-medium text-muted-foreground">
                  No products match your filters
                </p>
                <p className="text-sm text-muted-foreground mt-1">
                  Try adjusting your search or filter criteria
                </p>
              </div>
            )}

            {/* Pagination */}
            <Pagination
              page={pagination.page}
              pageSize={pagination.pageSize}
              totalPages={totalPages}
              totalCount={totalCount}
              onPageChange={setPage}
              onPageSizeChange={setPageSize}
            />
          </div>
        </div>
      </main>
    </div>
  );
}
