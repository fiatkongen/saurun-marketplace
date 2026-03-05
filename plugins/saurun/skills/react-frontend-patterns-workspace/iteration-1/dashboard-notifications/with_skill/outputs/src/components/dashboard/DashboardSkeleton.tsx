/**
 * DashboardSkeleton
 *
 * Loading skeleton shown while stats query is pending.
 * Component-level loading via isPending (per error/loading strategy).
 */

export function DashboardSkeleton() {
  return (
    <div
      className="space-y-6 animate-pulse"
      data-testid="dashboard-skeleton"
      aria-label="Loading dashboard"
    >
      {/* Tab bar skeleton */}
      <div className="flex gap-4 border-b pb-2">
        {[1, 2, 3].map((i) => (
          <div key={i} className="h-4 w-20 rounded bg-muted" />
        ))}
      </div>

      {/* Stat cards skeleton */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="rounded-lg border p-6 space-y-3">
            <div className="h-3 w-24 rounded bg-muted" />
            <div className="h-8 w-16 rounded bg-muted" />
            <div className="h-2 w-32 rounded bg-muted" />
          </div>
        ))}
      </div>

      {/* Table skeleton */}
      <div className="rounded-lg border p-4 space-y-3">
        <div className="h-4 w-32 rounded bg-muted" />
        {[1, 2, 3].map((i) => (
          <div key={i} className="flex justify-between">
            <div className="h-3 w-24 rounded bg-muted" />
            <div className="h-3 w-16 rounded bg-muted" />
          </div>
        ))}
      </div>
    </div>
  )
}
