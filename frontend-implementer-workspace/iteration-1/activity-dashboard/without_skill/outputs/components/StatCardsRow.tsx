// ============================================================
// StatCardsRow.tsx — Row of 4 stat cards with loading state
// ============================================================

import { useStatsQuery } from "../hooks";
import { StatCard } from "./StatCard";
import { StatCardSkeleton } from "./StatCardSkeleton";

export function StatCardsRow() {
  const { data, isLoading, isError } = useStatsQuery();

  if (isError) {
    return (
      <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
        Failed to load statistics. Please try again.
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {isLoading
        ? Array.from({ length: 4 }).map((_, i) => <StatCardSkeleton key={i} />)
        : data?.stats.map((stat) => <StatCard key={stat.id} data={stat} />)}
    </div>
  );
}
