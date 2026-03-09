// ============================================
// TanStack Query Hooks for Dashboard
//
// Server data lives here, NOT in Zustand.
// ============================================

import { useQuery, useInfiniteQuery } from "@tanstack/react-query"
import { dashboardApi } from "./api"
import { useFiltersSnapshot } from "./useDashboardFilterStore"
import type { ActivityFilters, ActivityFeedResponse } from "./types"

// ============================================
// Query Keys Factory
// ============================================

export const dashboardKeys = {
  all: ["dashboard"] as const,
  stats: (filters: ActivityFilters) =>
    [...dashboardKeys.all, "stats", filters] as const,
  activity: (filters: ActivityFilters) =>
    [...dashboardKeys.all, "activity", filters] as const,
}

// ============================================
// Stats Query
// ============================================

export function useDashboardStats() {
  const filters = useFiltersSnapshot()

  return useQuery({
    queryKey: dashboardKeys.stats(filters),
    queryFn: () => dashboardApi.stats.get(filters),
    staleTime: 60 * 1000, // 1 minute
  })
}

// ============================================
// Activity Feed (Infinite Query for Scroll)
// ============================================

export function useActivityFeed() {
  const filters = useFiltersSnapshot()

  return useInfiniteQuery<ActivityFeedResponse>({
    queryKey: dashboardKeys.activity(filters),
    queryFn: ({ pageParam }) =>
      dashboardApi.activity.list(filters, pageParam as string | undefined),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasMore ? lastPage.nextCursor : undefined,
    staleTime: 30 * 1000, // 30 seconds
  })
}
