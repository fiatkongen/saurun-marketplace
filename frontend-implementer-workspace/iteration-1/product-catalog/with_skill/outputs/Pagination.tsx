// =============================================================================
// Pagination — Page navigation with first/prev/next/last + page numbers
// =============================================================================

import { cn } from "./cn"

interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
  className?: string
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  className,
}: PaginationProps) {
  if (totalPages <= 1) return null

  // Calculate visible page numbers: show up to 5 centered on current
  const pages = getPageNumbers(currentPage, totalPages)

  return (
    <nav
      data-testid="catalog-pagination"
      className={cn("flex items-center justify-center gap-1", className)}
      aria-label="Pagination"
    >
      {/* Previous */}
      <button
        data-testid="catalog-pagination-prev"
        type="button"
        onClick={() => onPageChange(currentPage - 1)}
        disabled={currentPage <= 1}
        className={cn(
          "flex items-center gap-1 px-3 py-2 rounded-lg text-sm font-medium",
          "transition-colors duration-150",
          currentPage > 1
            ? "text-stone-600 hover:bg-stone-100 hover:text-stone-900"
            : "text-stone-300 cursor-not-allowed"
        )}
        aria-label="Previous page"
      >
        <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
          <path
            fillRule="evenodd"
            d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z"
            clipRule="evenodd"
          />
        </svg>
        <span className="hidden sm:inline">Prev</span>
      </button>

      {/* Page numbers */}
      <div className="flex items-center gap-0.5">
        {pages.map((page, index) => {
          if (page === "ellipsis") {
            return (
              <span
                key={`ellipsis-${index}`}
                className="w-9 h-9 flex items-center justify-center text-sm text-stone-400"
              >
                ...
              </span>
            )
          }

          const isActive = page === currentPage
          return (
            <button
              key={page}
              data-testid={`catalog-pagination-page-${page}`}
              type="button"
              onClick={() => onPageChange(page)}
              className={cn(
                "w-9 h-9 rounded-lg text-sm font-medium",
                "transition-all duration-150",
                isActive
                  ? "bg-stone-900 text-white shadow-xs"
                  : "text-stone-600 hover:bg-stone-100 hover:text-stone-900"
              )}
              aria-label={`Page ${page}`}
              aria-current={isActive ? "page" : undefined}
            >
              {page}
            </button>
          )
        })}
      </div>

      {/* Next */}
      <button
        data-testid="catalog-pagination-next"
        type="button"
        onClick={() => onPageChange(currentPage + 1)}
        disabled={currentPage >= totalPages}
        className={cn(
          "flex items-center gap-1 px-3 py-2 rounded-lg text-sm font-medium",
          "transition-colors duration-150",
          currentPage < totalPages
            ? "text-stone-600 hover:bg-stone-100 hover:text-stone-900"
            : "text-stone-300 cursor-not-allowed"
        )}
        aria-label="Next page"
      >
        <span className="hidden sm:inline">Next</span>
        <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor" aria-hidden="true">
          <path
            fillRule="evenodd"
            d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z"
            clipRule="evenodd"
          />
        </svg>
      </button>
    </nav>
  )
}

// ============================================
// Page number calculation with ellipsis
// ============================================

type PageItem = number | "ellipsis"

function getPageNumbers(current: number, total: number): PageItem[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1)
  }

  const pages: PageItem[] = []

  // Always show first page
  pages.push(1)

  if (current > 3) {
    pages.push("ellipsis")
  }

  // Pages around current
  const start = Math.max(2, current - 1)
  const end = Math.min(total - 1, current + 1)

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  if (current < total - 2) {
    pages.push("ellipsis")
  }

  // Always show last page
  pages.push(total)

  return pages
}
