// ============================================================================
// useCatalogParams — Sync filter/view/pagination state with URL query params
// ============================================================================

import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import type { FilterState, PaginationState, ViewMode } from "./types";
import { PRICE_MIN, PRICE_MAX } from "./data";

const DEFAULT_FILTERS: FilterState = {
  search: "",
  categories: [],
  priceRange: [PRICE_MIN, PRICE_MAX],
  minRating: 0,
  inStockOnly: false,
};

const DEFAULT_PAGINATION: PaginationState = {
  page: 1,
  pageSize: 12,
};

const DEFAULT_VIEW: ViewMode = "grid";

export function useCatalogParams() {
  const [searchParams, setSearchParams] = useSearchParams();

  const filters: FilterState = useMemo(() => {
    const cats = searchParams.get("categories");
    return {
      search: searchParams.get("search") ?? DEFAULT_FILTERS.search,
      categories: cats ? cats.split(",").filter(Boolean) : DEFAULT_FILTERS.categories,
      priceRange: [
        Number(searchParams.get("priceMin")) || PRICE_MIN,
        Number(searchParams.get("priceMax")) || PRICE_MAX,
      ] as [number, number],
      minRating: Number(searchParams.get("minRating")) || DEFAULT_FILTERS.minRating,
      inStockOnly: searchParams.get("inStock") === "true",
    };
  }, [searchParams]);

  const pagination: PaginationState = useMemo(
    () => ({
      page: Number(searchParams.get("page")) || DEFAULT_PAGINATION.page,
      pageSize: Number(searchParams.get("pageSize")) || DEFAULT_PAGINATION.pageSize,
    }),
    [searchParams]
  );

  const viewMode: ViewMode = useMemo(
    () => (searchParams.get("view") as ViewMode) ?? DEFAULT_VIEW,
    [searchParams]
  );

  const updateParams = useCallback(
    (updates: Record<string, string | undefined>) => {
      setSearchParams((prev) => {
        const next = new URLSearchParams(prev);
        for (const [key, value] of Object.entries(updates)) {
          if (value === undefined || value === "") {
            next.delete(key);
          } else {
            next.set(key, value);
          }
        }
        return next;
      });
    },
    [setSearchParams]
  );

  const setFilters = useCallback(
    (partial: Partial<FilterState>) => {
      const updates: Record<string, string | undefined> = {};

      if (partial.search !== undefined) {
        updates.search = partial.search || undefined;
      }
      if (partial.categories !== undefined) {
        updates.categories =
          partial.categories.length > 0 ? partial.categories.join(",") : undefined;
      }
      if (partial.priceRange !== undefined) {
        updates.priceMin =
          partial.priceRange[0] !== PRICE_MIN
            ? String(partial.priceRange[0])
            : undefined;
        updates.priceMax =
          partial.priceRange[1] !== PRICE_MAX
            ? String(partial.priceRange[1])
            : undefined;
      }
      if (partial.minRating !== undefined) {
        updates.minRating =
          partial.minRating > 0 ? String(partial.minRating) : undefined;
      }
      if (partial.inStockOnly !== undefined) {
        updates.inStock = partial.inStockOnly ? "true" : undefined;
      }

      // Reset to page 1 on filter change
      updates.page = undefined;

      updateParams(updates);
    },
    [updateParams]
  );

  const setViewMode = useCallback(
    (mode: ViewMode) => updateParams({ view: mode === DEFAULT_VIEW ? undefined : mode }),
    [updateParams]
  );

  const setPage = useCallback(
    (page: number) => updateParams({ page: page > 1 ? String(page) : undefined }),
    [updateParams]
  );

  const setPageSize = useCallback(
    (size: number) =>
      updateParams({
        pageSize: size !== DEFAULT_PAGINATION.pageSize ? String(size) : undefined,
        page: undefined,
      }),
    [updateParams]
  );

  return { filters, pagination, viewMode, setFilters, setViewMode, setPage, setPageSize };
}
