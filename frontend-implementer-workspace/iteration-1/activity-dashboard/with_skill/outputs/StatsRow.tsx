// ============================================
// StatsRow Component
//
// Horizontal row of 4 StatCards with loading state.
// ============================================

import { useDashboardStats } from "./useDashboardQueries"
import { StatCard, StatCardSkeleton } from "./StatCard"

export function StatsRow() {
  const { data, isPending, error } = useDashboardStats()

  if (error) {
    return (
      <div
        data-testid="stats-row-error"
        className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-3 font-[DM_Sans] text-sm text-red-400"
      >
        Failed to load statistics. {error.message}
      </div>
    )
  }

  if (isPending) {
    return (
      <div
        data-testid="stats-row-loading"
        className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
      >
        {Array.from({ length: 4 }).map((_, i) => (
          <StatCardSkeleton key={i} />
        ))}
      </div>
    )
  }

  return (
    <div
      data-testid="stats-row"
      className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
    >
      {data?.stats.map((stat) => (
        <StatCard key={stat.id} stat={stat} />
      ))}
    </div>
  )
}
