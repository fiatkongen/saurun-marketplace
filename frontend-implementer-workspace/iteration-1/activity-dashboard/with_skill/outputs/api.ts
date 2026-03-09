// ============================================
// API Client
// ============================================

import type {
  ActivityFeedResponse,
  ActivityFilters,
  DashboardStatsResponse,
} from "./types"

const BASE_URL = "/api/dashboard"

async function fetchJson<T>(url: string): Promise<T> {
  const res = await fetch(url)
  if (!res.ok) {
    throw new Error(`API error: ${res.status} ${res.statusText}`)
  }
  return res.json()
}

function buildQueryString(params: Record<string, string | undefined>): string {
  const entries = Object.entries(params).filter(
    (entry): entry is [string, string] => entry[1] !== undefined
  )
  if (entries.length === 0) return ""
  return "?" + new URLSearchParams(entries).toString()
}

export const dashboardApi = {
  stats: {
    get: (filters: ActivityFilters): Promise<DashboardStatsResponse> => {
      const qs = buildQueryString({
        dateFrom: filters.dateFrom ?? undefined,
        dateTo: filters.dateTo ?? undefined,
        types:
          filters.types.length > 0 ? filters.types.join(",") : undefined,
      })
      return fetchJson<DashboardStatsResponse>(`${BASE_URL}/stats${qs}`)
    },
  },

  activity: {
    list: (
      filters: ActivityFilters,
      cursor?: string
    ): Promise<ActivityFeedResponse> => {
      const qs = buildQueryString({
        dateFrom: filters.dateFrom ?? undefined,
        dateTo: filters.dateTo ?? undefined,
        types:
          filters.types.length > 0 ? filters.types.join(",") : undefined,
        cursor,
      })
      return fetchJson<ActivityFeedResponse>(`${BASE_URL}/activity${qs}`)
    },
  },
}
