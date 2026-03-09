// =============================================================================
// ViewToggle — Grid/List view toggle
// =============================================================================

import { cn } from "./cn"

interface ViewToggleProps {
  view: "grid" | "list"
  onChange: (view: "grid" | "list") => void
  className?: string
}

export function ViewToggle({ view, onChange, className }: ViewToggleProps) {
  return (
    <div
      className={cn("flex items-center gap-1 bg-stone-100 rounded-lg p-1", className)}
      role="radiogroup"
      aria-label="View mode"
    >
      <button
        data-testid="catalog-view-grid"
        type="button"
        role="radio"
        aria-checked={view === "grid"}
        onClick={() => onChange("grid")}
        className={cn(
          "p-1.5 rounded-md transition-all duration-150",
          view === "grid"
            ? "bg-white text-stone-900 shadow-xs"
            : "text-stone-400 hover:text-stone-600"
        )}
        aria-label="Grid view"
      >
        <svg className="w-4.5 h-4.5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
          <path
            fillRule="evenodd"
            d="M4.25 2A2.25 2.25 0 002 4.25v2.5A2.25 2.25 0 004.25 9h2.5A2.25 2.25 0 009 6.75v-2.5A2.25 2.25 0 006.75 2h-2.5zm0 9A2.25 2.25 0 002 13.25v2.5A2.25 2.25 0 004.25 18h2.5A2.25 2.25 0 009 15.75v-2.5A2.25 2.25 0 006.75 11h-2.5zm9-9A2.25 2.25 0 0011 4.25v2.5A2.25 2.25 0 0013.25 9h2.5A2.25 2.25 0 0018 6.75v-2.5A2.25 2.25 0 0015.75 2h-2.5zm0 9A2.25 2.25 0 0011 13.25v2.5A2.25 2.25 0 0013.25 18h2.5A2.25 2.25 0 0018 15.75v-2.5A2.25 2.25 0 0015.75 11h-2.5z"
            clipRule="evenodd"
          />
        </svg>
      </button>
      <button
        data-testid="catalog-view-list"
        type="button"
        role="radio"
        aria-checked={view === "list"}
        onClick={() => onChange("list")}
        className={cn(
          "p-1.5 rounded-md transition-all duration-150",
          view === "list"
            ? "bg-white text-stone-900 shadow-xs"
            : "text-stone-400 hover:text-stone-600"
        )}
        aria-label="List view"
      >
        <svg className="w-4.5 h-4.5" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
          <path
            fillRule="evenodd"
            d="M2 3.75A.75.75 0 012.75 3h14.5a.75.75 0 010 1.5H2.75A.75.75 0 012 3.75zm0 4.167a.75.75 0 01.75-.75h14.5a.75.75 0 010 1.5H2.75a.75.75 0 01-.75-.75zm0 4.166a.75.75 0 01.75-.75h14.5a.75.75 0 010 1.5H2.75a.75.75 0 01-.75-.75zm0 4.167a.75.75 0 01.75-.75h14.5a.75.75 0 010 1.5H2.75a.75.75 0 01-.75-.75z"
            clipRule="evenodd"
          />
        </svg>
      </button>
    </div>
  )
}
