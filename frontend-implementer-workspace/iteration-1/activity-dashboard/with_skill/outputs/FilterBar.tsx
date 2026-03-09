// ============================================
// FilterBar Component
//
// Date range picker + activity type multi-select.
// Uses Zustand filter store for state.
// ============================================

import { useState, useRef, useEffect } from "react"
import { cn } from "./cn"
import {
  useDateRange,
  useSelectedTypes,
  useFilterActions,
} from "./useDashboardFilterStore"
import type { ActivityType } from "./types"

// ============================================
// Activity Type Multi-Select
// ============================================

const activityTypeOptions: { value: ActivityType; label: string; color: string }[] = [
  { value: "login", label: "Login", color: "bg-sky-500" },
  { value: "purchase", label: "Purchase", color: "bg-emerald-500" },
  { value: "signup", label: "Signup", color: "bg-amber-500" },
]

function TypeMultiSelect() {
  const selectedTypes = useSelectedTypes()
  const { toggleType } = useFilterActions()
  const [isOpen, setIsOpen] = useState(false)
  const dropdownRef = useRef<HTMLDivElement>(null)

  // Close on outside click
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (
        dropdownRef.current &&
        !dropdownRef.current.contains(e.target as Node)
      ) {
        setIsOpen(false)
      }
    }
    document.addEventListener("mousedown", handleClickOutside)
    return () => document.removeEventListener("mousedown", handleClickOutside)
  }, [])

  const label =
    selectedTypes.length === 0
      ? "All types"
      : selectedTypes.length === activityTypeOptions.length
        ? "All types"
        : `${selectedTypes.length} selected`

  return (
    <div ref={dropdownRef} className="relative">
      <button
        data-testid="filter-type-select"
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          "flex items-center gap-2 rounded-md border border-zinc-800",
          "bg-zinc-900/80 px-3 py-2",
          "font-[DM_Sans] text-sm text-zinc-300",
          "transition-colors hover:border-zinc-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-500/50"
        )}
      >
        <svg
          width="14"
          height="14"
          viewBox="0 0 14 14"
          fill="none"
          className="text-zinc-500"
          aria-hidden="true"
        >
          <path
            d="M1 3h12M3 7h8M5 11h4"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
          />
        </svg>
        {label}
        <svg
          width="10"
          height="10"
          viewBox="0 0 10 10"
          fill="none"
          className={cn(
            "text-zinc-500 transition-transform",
            isOpen && "rotate-180"
          )}
          aria-hidden="true"
        >
          <path
            d="M2 4l3 3 3-3"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      </button>

      {isOpen && (
        <div
          data-testid="filter-type-dropdown"
          className={cn(
            "absolute left-0 top-full z-10 mt-1 w-48",
            "rounded-md border border-zinc-800 bg-zinc-900 py-1 shadow-xl shadow-black/30"
          )}
        >
          {activityTypeOptions.map((option) => {
            const isSelected = selectedTypes.includes(option.value)
            return (
              <button
                key={option.value}
                data-testid={`filter-type-option-${option.value}`}
                type="button"
                onClick={() => toggleType(option.value)}
                className={cn(
                  "flex w-full items-center gap-2.5 px-3 py-2",
                  "text-left font-[DM_Sans] text-sm transition-colors",
                  isSelected
                    ? "bg-zinc-800/50 text-zinc-200"
                    : "text-zinc-400 hover:bg-zinc-800/30 hover:text-zinc-300"
                )}
              >
                {/* Checkbox indicator */}
                <span
                  className={cn(
                    "flex h-4 w-4 items-center justify-center rounded-sm border",
                    isSelected
                      ? "border-amber-500 bg-amber-500"
                      : "border-zinc-700"
                  )}
                >
                  {isSelected && (
                    <svg
                      width="10"
                      height="10"
                      viewBox="0 0 10 10"
                      fill="none"
                      aria-hidden="true"
                    >
                      <path
                        d="M2 5l2.5 2.5L8 3"
                        stroke="currentColor"
                        strokeWidth="1.5"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        className="text-zinc-900"
                      />
                    </svg>
                  )}
                </span>

                {/* Color dot */}
                <span
                  className={cn("h-2 w-2 rounded-full", option.color)}
                  aria-hidden="true"
                />

                {option.label}
              </button>
            )
          })}
        </div>
      )}
    </div>
  )
}

// ============================================
// Date Range Picker
// ============================================

function DateRangePicker() {
  const { dateFrom, dateTo } = useDateRange()
  const { setDateRange } = useFilterActions()

  return (
    <div className="flex items-center gap-2">
      <label
        htmlFor="date-from"
        className="font-[DM_Sans] text-xs font-medium uppercase tracking-wider text-zinc-500"
      >
        From
      </label>
      <input
        id="date-from"
        data-testid="filter-date-from"
        type="date"
        value={dateFrom ?? ""}
        onChange={(e) => setDateRange(e.target.value || null, dateTo)}
        className={cn(
          "rounded-md border border-zinc-800 bg-zinc-900/80 px-2.5 py-1.5",
          "font-[JetBrains_Mono] text-sm text-zinc-300",
          "transition-colors hover:border-zinc-700",
          "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-500/50",
          "[color-scheme:dark]"
        )}
      />

      <label
        htmlFor="date-to"
        className="font-[DM_Sans] text-xs font-medium uppercase tracking-wider text-zinc-500"
      >
        To
      </label>
      <input
        id="date-to"
        data-testid="filter-date-to"
        type="date"
        value={dateTo ?? ""}
        onChange={(e) => setDateRange(dateFrom, e.target.value || null)}
        className={cn(
          "rounded-md border border-zinc-800 bg-zinc-900/80 px-2.5 py-1.5",
          "font-[JetBrains_Mono] text-sm text-zinc-300",
          "transition-colors hover:border-zinc-700",
          "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-500/50",
          "[color-scheme:dark]"
        )}
      />
    </div>
  )
}

// ============================================
// Main FilterBar
// ============================================

export function FilterBar() {
  const { clearFilters } = useFilterActions()
  const selectedTypes = useSelectedTypes()
  const { dateFrom, dateTo } = useDateRange()

  const hasActiveFilters =
    selectedTypes.length > 0 || dateFrom !== null || dateTo !== null

  return (
    <div
      data-testid="filter-bar"
      className={cn(
        "flex flex-wrap items-center gap-4 rounded-lg border border-zinc-800/60",
        "bg-zinc-900/30 px-4 py-3"
      )}
    >
      <DateRangePicker />

      <div className="h-5 w-px bg-zinc-800" aria-hidden="true" />

      <TypeMultiSelect />

      {hasActiveFilters && (
        <button
          data-testid="filter-clear"
          type="button"
          onClick={clearFilters}
          className={cn(
            "ml-auto rounded-md px-2.5 py-1.5",
            "font-[DM_Sans] text-xs font-medium text-zinc-500",
            "transition-colors hover:bg-zinc-800 hover:text-zinc-300",
            "focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-500/50"
          )}
        >
          Clear filters
        </button>
      )}
    </div>
  )
}
