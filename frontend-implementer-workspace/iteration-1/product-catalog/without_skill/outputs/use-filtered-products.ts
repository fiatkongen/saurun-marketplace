// ============================================================================
// useFilteredProducts — Filter + paginate products
// ============================================================================

import { useMemo } from "react";
import type { FilterState, PaginationState, Product } from "./types";
import { MOCK_PRODUCTS } from "./data";

interface UseFilteredProductsResult {
  products: Product[];
  totalCount: number;
  totalPages: number;
}

export function useFilteredProducts(
  filters: FilterState,
  pagination: PaginationState
): UseFilteredProductsResult {
  const filtered = useMemo(() => {
    let result = [...MOCK_PRODUCTS];

    // Search
    if (filters.search) {
      const q = filters.search.toLowerCase();
      result = result.filter(
        (p) =>
          p.title.toLowerCase().includes(q) ||
          p.category.toLowerCase().includes(q)
      );
    }

    // Categories
    if (filters.categories.length > 0) {
      result = result.filter((p) => filters.categories.includes(p.category));
    }

    // Price range
    result = result.filter(
      (p) => p.price >= filters.priceRange[0] && p.price <= filters.priceRange[1]
    );

    // Rating
    if (filters.minRating > 0) {
      result = result.filter((p) => p.rating >= filters.minRating);
    }

    // In stock
    if (filters.inStockOnly) {
      result = result.filter((p) => p.inStock);
    }

    return result;
  }, [filters]);

  const totalCount = filtered.length;
  const totalPages = Math.max(1, Math.ceil(totalCount / pagination.pageSize));
  const start = (pagination.page - 1) * pagination.pageSize;
  const products = filtered.slice(start, start + pagination.pageSize);

  return { products, totalCount, totalPages };
}
