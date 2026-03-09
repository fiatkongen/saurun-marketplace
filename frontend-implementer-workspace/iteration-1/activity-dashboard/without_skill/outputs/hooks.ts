// ============================================================
// hooks.ts — TanStack Query hooks for the Activity Dashboard
// ============================================================

import { useInfiniteQuery, useQuery } from "@tanstack/react-query";
import { fetchActivityFeed, fetchStats } from "./api";
import { useDashboardStore } from "./store";
import type { ActivityFeedResponse, StatsResponse } from "./types";

// ---------------------------------------------------------------------------
// Stats query
// ---------------------------------------------------------------------------

export function useStatsQuery() {
  return useQuery<StatsResponse>({
    queryKey: ["dashboard-stats"],
    queryFn: fetchStats,
    staleTime: 30_000, // 30 seconds
    refetchInterval: 60_000, // auto-refresh every 60s
  });
}

// ---------------------------------------------------------------------------
// Activity feed infinite query
// ---------------------------------------------------------------------------

export function useActivityFeedQuery() {
  const { dateRange, selectedActivityTypes } = useDashboardStore();

  return useInfiniteQuery<ActivityFeedResponse>({
    queryKey: ["activity-feed", { dateRange, activityTypes: selectedActivityTypes }],
    queryFn: ({ pageParam }) =>
      fetchActivityFeed({
        cursor: pageParam as string | null,
        pageSize: 20,
        activityTypes: selectedActivityTypes.length > 0 ? selectedActivityTypes : undefined,
        dateRange,
      }),
    initialPageParam: null as string | null,
    getNextPageParam: (lastPage) => lastPage.nextCursor,
    staleTime: 15_000,
  });
}

// ---------------------------------------------------------------------------
// Utility: flatten pages into a single array
// ---------------------------------------------------------------------------

export function useFlatActivityItems() {
  const query = useActivityFeedQuery();
  const items = query.data?.pages.flatMap((page) => page.items) ?? [];
  return { ...query, items };
}

// ---------------------------------------------------------------------------
// Relative timestamp formatter
// ---------------------------------------------------------------------------

const rtf = new Intl.RelativeTimeFormat("en", { numeric: "auto" });

const DIVISIONS: { amount: number; name: Intl.RelativeTimeFormatUnit }[] = [
  { amount: 60, name: "seconds" },
  { amount: 60, name: "minutes" },
  { amount: 24, name: "hours" },
  { amount: 7, name: "days" },
  { amount: 4.34524, name: "weeks" },
  { amount: 12, name: "months" },
  { amount: Number.POSITIVE_INFINITY, name: "years" },
];

export function formatRelativeTime(dateString: string): string {
  let duration = (new Date(dateString).getTime() - Date.now()) / 1000;

  for (const division of DIVISIONS) {
    if (Math.abs(duration) < division.amount) {
      return rtf.format(Math.round(duration), division.name);
    }
    duration /= division.amount;
  }

  return dateString;
}
