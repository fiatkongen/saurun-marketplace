// =============================================================================
// useFilterParams — URL Query Param Sync for Filters
// Reads/writes filter state to URL search params so links are shareable.
// =============================================================================

import { useCallback, useMemo } from "react"
import { useSearchParams } from "react-router-dom"
import {
  DEFAULT_FILTERS,
  PRICE_MIN,
  PRICE_MAX,
  type ProductFilters,
} from "./types"

function parseNumber(val: string | null, fallback: number): number {
  if (val === null) return fallback
  const n = Number(val)
  return Number.isFinite(n) ? n : fallback
}

function parseView(val: string | null): "grid" | "list" {
  return val === "list" ? "list" : "grid"
}

function parseSortBy(
  val: string | null
): ProductFilters["sortBy"] {
  const valid: ProductFilters["sortBy"][] = [
    "price-asc",
    "price-desc",
    "rating",
    "name",
    "newest",
  ]
  return valid.includes(val as ProductFilters["sortBy"])
    ? (val as ProductFilters["sortBy"])
    : "newest"
}

export function useFilterParams() {
  const [searchParams, setSearchParams] = useSearchParams()

  const filters: ProductFilters = useMemo(() => {
    const categories = searchParams.get("categories")
    return {
      search: searchParams.get("q") ?? DEFAULT_FILTERS.search,
      categories: categories ? categories.split(",").filter(Boolean) : [],
      priceMin: parseNumber(searchParams.get("priceMin"), PRICE_MIN),
      priceMax: parseNumber(searchParams.get("priceMax"), PRICE_MAX),
      minRating: parseNumber(searchParams.get("minRating"), 0),
      inStockOnly: searchParams.get("inStock") === "true",
      page: parseNumber(searchParams.get("page"), 1),
      view: parseView(searchParams.get("view")),
      sortBy: parseSortBy(searchParams.get("sortBy")),
    }
  }, [searchParams])

  const setFilters = useCallback(
    (updates: Partial<ProductFilters>) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev)
          const merged = { ...filters, ...updates }

          // Reset page to 1 when any filter changes (except page/view)
          const isFilterChange = Object.keys(updates).some(
            (k) => k !== "page" && k !== "view"
          )
          if (isFilterChange && !("page" in updates)) {
            merged.page = 1
          }

          // Set or delete each param
          if (merged.search) {
            next.set("q", merged.search)
          } else {
            next.delete("q")
          }

          if (merged.categories.length > 0) {
            next.set("categories", merged.categories.join(","))
          } else {
            next.delete("categories")
          }

          if (merged.priceMin !== PRICE_MIN) {
            next.set("priceMin", String(merged.priceMin))
          } else {
            next.delete("priceMin")
          }

          if (merged.priceMax !== PRICE_MAX) {
            next.set("priceMax", String(merged.priceMax))
          } else {
            next.delete("priceMax")
          }

          if (merged.minRating > 0) {
            next.set("minRating", String(merged.minRating))
          } else {
            next.delete("minRating")
          }

          if (merged.inStockOnly) {
            next.set("inStock", "true")
          } else {
            next.delete("inStock")
          }

          if (merged.page > 1) {
            next.set("page", String(merged.page))
          } else {
            next.delete("page")
          }

          if (merged.view !== "grid") {
            next.set("view", merged.view)
          } else {
            next.delete("view")
          }

          if (merged.sortBy !== "newest") {
            next.set("sortBy", merged.sortBy)
          } else {
            next.delete("sortBy")
          }

          return next
        },
        { replace: true }
      )
    },
    [filters, setSearchParams]
  )

  return { filters, setFilters }
}
