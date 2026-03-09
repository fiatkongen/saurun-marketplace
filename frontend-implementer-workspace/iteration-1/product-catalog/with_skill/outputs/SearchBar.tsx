// =============================================================================
// SearchBar — Debounced search input synced to URL params
// =============================================================================

import { useState, useEffect } from "react"
import { cn } from "./cn"
import { useDebouncedValue } from "./useDebouncedValue"

interface SearchBarProps {
  value: string
  onChange: (value: string) => void
  className?: string
}

export function SearchBar({ value, onChange, className }: SearchBarProps) {
  const [localValue, setLocalValue] = useState(value)
  const debouncedValue = useDebouncedValue(localValue, 300)

  // Sync debounced value upstream
  useEffect(() => {
    if (debouncedValue !== value) {
      onChange(debouncedValue)
    }
  }, [debouncedValue, onChange, value])

  // Sync external value changes (e.g., browser back/forward)
  useEffect(() => {
    setLocalValue(value)
  }, [value])

  return (
    <div className={cn("relative", className)}>
      <svg
        className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4.5 h-4.5 text-stone-400 pointer-events-none"
        viewBox="0 0 20 20"
        fill="currentColor"
        aria-hidden="true"
      >
        <path
          fillRule="evenodd"
          d="M9 3.5a5.5 5.5 0 100 11 5.5 5.5 0 000-11zM2 9a7 7 0 1112.452 4.391l3.328 3.329a.75.75 0 11-1.06 1.06l-3.329-3.328A7 7 0 012 9z"
          clipRule="evenodd"
        />
      </svg>
      <input
        data-testid="catalog-search-input"
        type="text"
        value={localValue}
        onChange={(e) => setLocalValue(e.target.value)}
        placeholder="Search products..."
        className={cn(
          "w-full h-11 pl-11 pr-4",
          "bg-stone-50 border border-stone-200 rounded-lg",
          "text-sm text-stone-900 placeholder:text-stone-400",
          "transition-all duration-200",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-amber-500/40 focus-visible:border-amber-500",
          "hover:border-stone-300"
        )}
        aria-label="Search products"
      />
      {localValue && (
        <button
          data-testid="catalog-search-clear"
          type="button"
          onClick={() => {
            setLocalValue("")
            onChange("")
          }}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-stone-400 hover:text-stone-600 transition-colors"
          aria-label="Clear search"
        >
          <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor">
            <path
              fillRule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
              clipRule="evenodd"
            />
          </svg>
        </button>
      )}
    </div>
  )
}
