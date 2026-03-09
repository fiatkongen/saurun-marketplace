// ============================================
// StatCard Component
//
// Displays a single metric with trend indicator.
// Subtle hover: scale + glow + border illuminate.
// ============================================

import { cn } from "./cn"
import type { StatCard as StatCardType, TrendDirection } from "./types"

// ============================================
// Trend Arrow SVG
// ============================================

function TrendArrow({ direction }: { direction: TrendDirection }) {
  if (direction === "flat") {
    return (
      <svg
        width="14"
        height="14"
        viewBox="0 0 14 14"
        fill="none"
        aria-hidden="true"
        className="text-zinc-500"
      >
        <path
          d="M2 7h10"
          stroke="currentColor"
          strokeWidth="1.5"
          strokeLinecap="round"
        />
      </svg>
    )
  }

  const isUp = direction === "up"

  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 14 14"
      fill="none"
      aria-hidden="true"
      className={isUp ? "text-emerald-400" : "text-red-400"}
    >
      <path
        d={isUp ? "M7 11V3m0 0l-3 3m3-3l3 3" : "M7 3v8m0 0l-3-3m3 3l3-3"}
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

// ============================================
// Skeleton Loader
// ============================================

export function StatCardSkeleton() {
  return (
    <div
      className={cn(
        "relative overflow-hidden rounded-lg border border-zinc-800/60",
        "bg-zinc-900/50 p-5"
      )}
    >
      <div className="mb-3 h-3.5 w-24 animate-pulse rounded-sm bg-zinc-800" />
      <div className="mb-2 h-8 w-32 animate-pulse rounded-sm bg-zinc-800" />
      <div className="h-3.5 w-20 animate-pulse rounded-sm bg-zinc-800" />
    </div>
  )
}

// ============================================
// Main Component
// ============================================

interface StatCardProps {
  stat: StatCardType
}

export function StatCard({ stat }: StatCardProps) {
  const trendColor: Record<TrendDirection, string> = {
    up: "text-emerald-400",
    down: "text-red-400",
    flat: "text-zinc-500",
  }

  return (
    <div
      data-testid={`stat-card-${stat.id}`}
      className={cn(
        "group relative overflow-hidden rounded-lg border border-zinc-800/60",
        "bg-zinc-900/50 p-5",
        "transition-all duration-300 ease-out",
        "hover:scale-[1.02] hover:border-amber-500/30",
        "hover:bg-zinc-900/80 hover:shadow-lg hover:shadow-amber-500/5"
      )}
    >
      {/* Subtle gradient overlay on hover */}
      <div
        className={cn(
          "pointer-events-none absolute inset-0",
          "bg-gradient-to-br from-amber-500/0 to-transparent",
          "transition-opacity duration-300",
          "opacity-0 group-hover:opacity-[0.04]"
        )}
      />

      <p className="mb-1 font-[DM_Sans] text-xs font-medium uppercase tracking-wider text-zinc-500">
        {stat.label}
      </p>

      <p className="mb-2 font-[JetBrains_Mono] text-2xl font-semibold tracking-tight text-zinc-100">
        {stat.prefix ?? ""}
        {stat.value}
      </p>

      <div className="flex items-center gap-1.5">
        <TrendArrow direction={stat.trend} />
        <span
          className={cn(
            "font-[JetBrains_Mono] text-xs font-medium",
            trendColor[stat.trend]
          )}
        >
          {stat.trendPercent > 0 ? "+" : ""}
          {stat.trendPercent}%
        </span>
        <span className="text-xs text-zinc-600">vs last period</span>
      </div>
    </div>
  )
}
