// ============================================
// DashboardPage — Root Page Component
//
// Composes: FilterBar, StatsRow, ActivityFeed.
// Design: "Data Observatory" — dark, editorial,
// JetBrains Mono headers + DM Sans body,
// warm amber accents on zinc.
// ============================================

import { FilterBar } from "./FilterBar"
import { StatsRow } from "./StatsRow"
import { ActivityFeed } from "./ActivityFeed"

export function DashboardPage() {
  return (
    <div className="min-h-screen bg-zinc-950 text-zinc-100">
      {/* Ambient background grain */}
      <div
        className="pointer-events-none fixed inset-0 opacity-[0.03]"
        style={{
          backgroundImage:
            "url(\"data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.9' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E\")",
        }}
        aria-hidden="true"
      />

      {/* Top bar accent line */}
      <div
        className="h-px bg-gradient-to-r from-transparent via-amber-500/40 to-transparent"
        aria-hidden="true"
      />

      <div className="relative mx-auto max-w-6xl px-4 py-8 sm:px-6 lg:px-8">
        {/* Header */}
        <header className="mb-8">
          <h1 className="font-[JetBrains_Mono] text-2xl font-bold tracking-tight text-zinc-100">
            Dashboard
          </h1>
          <p className="mt-1 font-[DM_Sans] text-sm text-zinc-500">
            Overview of platform activity and key metrics.
          </p>
        </header>

        {/* Filter Bar */}
        <section className="mb-6" aria-label="Filters">
          <FilterBar />
        </section>

        {/* Stat Cards */}
        <section className="mb-8" aria-label="Statistics">
          <StatsRow />
        </section>

        {/* Activity Feed */}
        <section
          className="rounded-lg border border-zinc-800/60 bg-zinc-900/20 p-5"
          aria-label="Activity feed"
        >
          <ActivityFeed />
        </section>
      </div>
    </div>
  )
}
