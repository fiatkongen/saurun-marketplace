// =============================================================================
// FilterSidebar — Collapsible filter groups for the product catalog
// Includes: Price range, Category checkboxes, Rating filter, In-stock toggle
// =============================================================================

import { useState } from "react"
import { cn } from "./cn"
import { RatingStars } from "./RatingStars"
import { CATEGORIES, PRICE_MIN, PRICE_MAX, type ProductFilters } from "./types"

// ============================================
// Collapsible Group Wrapper
// ============================================

function FilterGroup({
  title,
  defaultOpen = true,
  children,
  testId,
}: {
  title: string
  defaultOpen?: boolean
  children: React.ReactNode
  testId: string
}) {
  const [isOpen, setIsOpen] = useState(defaultOpen)

  return (
    <div className="border-b border-stone-200 last:border-b-0" data-testid={testId}>
      <button
        data-testid={`${testId}-toggle`}
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          "flex w-full items-center justify-between py-3.5 px-0.5",
          "text-sm font-semibold tracking-wide uppercase text-stone-700",
          "transition-colors hover:text-stone-900"
        )}
        aria-expanded={isOpen}
      >
        <span>{title}</span>
        <svg
          className={cn(
            "w-4 h-4 text-stone-400 transition-transform duration-200",
            isOpen && "rotate-180"
          )}
          viewBox="0 0 20 20"
          fill="currentColor"
          aria-hidden="true"
        >
          <path
            fillRule="evenodd"
            d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z"
            clipRule="evenodd"
          />
        </svg>
      </button>
      <div
        className={cn(
          "overflow-hidden transition-all duration-200",
          isOpen ? "max-h-96 pb-4 opacity-100" : "max-h-0 opacity-0"
        )}
      >
        {children}
      </div>
    </div>
  )
}

// ============================================
// Price Range Slider
// ============================================

function PriceRangeFilter({
  priceMin,
  priceMax,
  onChange,
}: {
  priceMin: number
  priceMax: number
  onChange: (min: number, max: number) => void
}) {
  return (
    <div className="space-y-3 px-0.5">
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <span className="absolute left-2.5 top-1/2 -translate-y-1/2 text-xs text-stone-400">$</span>
          <input
            data-testid="filter-price-min"
            type="number"
            min={PRICE_MIN}
            max={priceMax}
            value={priceMin}
            onChange={(e) => onChange(Number(e.target.value), priceMax)}
            className={cn(
              "w-full h-9 pl-6 pr-2 rounded-md border border-stone-200 bg-stone-50",
              "text-sm text-stone-700",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/40 focus-visible:border-amber-500"
            )}
            aria-label="Minimum price"
          />
        </div>
        <span className="text-xs text-stone-400 font-medium">&ndash;</span>
        <div className="relative flex-1">
          <span className="absolute left-2.5 top-1/2 -translate-y-1/2 text-xs text-stone-400">$</span>
          <input
            data-testid="filter-price-max"
            type="number"
            min={priceMin}
            max={PRICE_MAX}
            value={priceMax}
            onChange={(e) => onChange(priceMin, Number(e.target.value))}
            className={cn(
              "w-full h-9 pl-6 pr-2 rounded-md border border-stone-200 bg-stone-50",
              "text-sm text-stone-700",
              "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/40 focus-visible:border-amber-500"
            )}
            aria-label="Maximum price"
          />
        </div>
      </div>
      <input
        data-testid="filter-price-range-slider"
        type="range"
        min={PRICE_MIN}
        max={PRICE_MAX}
        value={priceMax}
        onChange={(e) => onChange(priceMin, Number(e.target.value))}
        className={cn(
          "w-full h-1.5 rounded-full appearance-none cursor-pointer",
          "bg-stone-200 accent-amber-600"
        )}
        aria-label="Price range"
      />
      <div className="flex justify-between text-xs text-stone-400">
        <span>${PRICE_MIN}</span>
        <span>${PRICE_MAX}</span>
      </div>
    </div>
  )
}

// ============================================
// Category Checkboxes
// ============================================

function CategoryFilter({
  selected,
  onChange,
}: {
  selected: string[]
  onChange: (categories: string[]) => void
}) {
  const toggle = (category: string) => {
    if (selected.includes(category)) {
      onChange(selected.filter((c) => c !== category))
    } else {
      onChange([...selected, category])
    }
  }

  return (
    <div className="space-y-1.5 px-0.5">
      {CATEGORIES.map((category) => (
        <label
          key={category}
          className={cn(
            "flex items-center gap-2.5 py-1 px-1 rounded-md cursor-pointer",
            "text-sm text-stone-600 transition-colors",
            "hover:bg-stone-50 hover:text-stone-900",
            selected.includes(category) && "text-stone-900 font-medium"
          )}
        >
          <input
            data-testid={`filter-category-${category.toLowerCase().replace(/\s+&?\s*/g, "-")}`}
            type="checkbox"
            checked={selected.includes(category)}
            onChange={() => toggle(category)}
            className={cn(
              "w-4 h-4 rounded border-stone-300",
              "text-amber-600 focus:ring-amber-500/40 focus:ring-2"
            )}
          />
          <span>{category}</span>
        </label>
      ))}
    </div>
  )
}

// ============================================
// Rating Filter
// ============================================

function RatingFilter({
  minRating,
  onChange,
}: {
  minRating: number
  onChange: (rating: number) => void
}) {
  return (
    <div className="space-y-1 px-0.5">
      {[4, 3, 2, 1].map((rating) => (
        <button
          key={rating}
          data-testid={`filter-rating-${rating}`}
          type="button"
          onClick={() => onChange(minRating === rating ? 0 : rating)}
          className={cn(
            "flex items-center gap-2 w-full py-1.5 px-1 rounded-md transition-colors",
            "text-sm text-stone-600",
            "hover:bg-stone-50",
            minRating === rating && "bg-amber-50 text-amber-800"
          )}
        >
          <RatingStars rating={rating} size="sm" />
          <span>& up</span>
        </button>
      ))}
    </div>
  )
}

// ============================================
// In-Stock Toggle
// ============================================

function InStockToggle({
  checked,
  onChange,
}: {
  checked: boolean
  onChange: (checked: boolean) => void
}) {
  return (
    <label className="flex items-center gap-3 px-0.5 cursor-pointer">
      <button
        data-testid="filter-in-stock-toggle"
        type="button"
        role="switch"
        aria-checked={checked}
        onClick={() => onChange(!checked)}
        className={cn(
          "relative inline-flex h-6 w-11 shrink-0 rounded-full border-2 border-transparent",
          "transition-colors duration-200 cursor-pointer",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/40 focus-visible:ring-offset-2",
          checked ? "bg-amber-600" : "bg-stone-200"
        )}
      >
        <span
          className={cn(
            "pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow-sm",
            "transition-transform duration-200",
            checked ? "translate-x-5" : "translate-x-0"
          )}
        />
      </button>
      <span className="text-sm text-stone-600">In stock only</span>
    </label>
  )
}

// ============================================
// Sidebar Composite
// ============================================

interface FilterSidebarProps {
  filters: ProductFilters
  onFilterChange: (updates: Partial<ProductFilters>) => void
  className?: string
}

export function FilterSidebar({
  filters,
  onFilterChange,
  className,
}: FilterSidebarProps) {
  return (
    <aside
      data-testid="catalog-filter-sidebar"
      className={cn("w-64 shrink-0", className)}
    >
      <div className="sticky top-6">
        <h2 className="text-xs font-bold uppercase tracking-widest text-stone-400 mb-2 px-0.5">
          Filters
        </h2>

        <FilterGroup title="Price Range" testId="filter-group-price">
          <PriceRangeFilter
            priceMin={filters.priceMin}
            priceMax={filters.priceMax}
            onChange={(min, max) =>
              onFilterChange({ priceMin: min, priceMax: max })
            }
          />
        </FilterGroup>

        <FilterGroup title="Category" testId="filter-group-category">
          <CategoryFilter
            selected={filters.categories}
            onChange={(categories) => onFilterChange({ categories })}
          />
        </FilterGroup>

        <FilterGroup title="Rating" testId="filter-group-rating">
          <RatingFilter
            minRating={filters.minRating}
            onChange={(minRating) => onFilterChange({ minRating })}
          />
        </FilterGroup>

        <FilterGroup title="Availability" testId="filter-group-availability">
          <InStockToggle
            checked={filters.inStockOnly}
            onChange={(inStockOnly) => onFilterChange({ inStockOnly })}
          />
        </FilterGroup>

        {/* Clear all filters */}
        <button
          data-testid="filter-clear-all"
          type="button"
          onClick={() =>
            onFilterChange({
              categories: [],
              priceMin: PRICE_MIN,
              priceMax: PRICE_MAX,
              minRating: 0,
              inStockOnly: false,
              search: "",
            })
          }
          className={cn(
            "w-full mt-4 py-2 px-3 rounded-md text-sm font-medium",
            "text-stone-500 hover:text-stone-700 hover:bg-stone-100",
            "transition-colors"
          )}
        >
          Clear all filters
        </button>
      </div>
    </aside>
  )
}
