// =============================================================================
// SortSelect — Dropdown for sort order
// =============================================================================

import { cn } from "./cn"
import type { ProductFilters } from "./types"

interface SortSelectProps {
  value: ProductFilters["sortBy"]
  onChange: (value: ProductFilters["sortBy"]) => void
  className?: string
}

const SORT_OPTIONS: { value: ProductFilters["sortBy"]; label: string }[] = [
  { value: "newest", label: "Newest" },
  { value: "price-asc", label: "Price: Low to High" },
  { value: "price-desc", label: "Price: High to Low" },
  { value: "rating", label: "Highest Rated" },
  { value: "name", label: "Name A-Z" },
]

export function SortSelect({ value, onChange, className }: SortSelectProps) {
  return (
    <div className={cn("flex items-center gap-2", className)}>
      <label
        htmlFor="sort-select"
        className="text-xs font-medium text-stone-400 uppercase tracking-wide whitespace-nowrap"
      >
        Sort by
      </label>
      <select
        id="sort-select"
        data-testid="catalog-sort-select"
        value={value}
        onChange={(e) => onChange(e.target.value as ProductFilters["sortBy"])}
        className={cn(
          "h-9 px-3 pr-8 rounded-md border border-stone-200 bg-stone-50",
          "text-sm text-stone-700 appearance-none cursor-pointer",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/40 focus-visible:border-amber-500",
          "bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20viewBox%3D%220%200%2020%2020%22%20fill%3D%22%2378716c%22%3E%3Cpath%20fill-rule%3D%22evenodd%22%20d%3D%22M5.23%207.21a.75.75%200%20011.06.02L10%2011.168l3.71-3.938a.75.75%200%20111.08%201.04l-4.25%204.5a.75.75%200%2001-1.08%200l-4.25-4.5a.75.75%200%2001.02-1.06z%22%20clip-rule%3D%22evenodd%22%2F%3E%3C%2Fsvg%3E')]",
          "bg-[length:1.25rem] bg-[position:right_0.375rem_center] bg-no-repeat"
        )}
      >
        {SORT_OPTIONS.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
    </div>
  )
}
