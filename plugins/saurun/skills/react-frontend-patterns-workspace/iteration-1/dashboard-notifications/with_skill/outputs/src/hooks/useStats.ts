/**
 * Stats Query Hook
 *
 * TanStack Query hook for fetching user statistics from /api/stats.
 * Server data → TanStack Query cache (per state colocation rules).
 * Query keys follow [domain, action, params] pattern.
 */

import { useQuery } from '@tanstack/react-query'
import type { UserStats } from '@/types/stats'

// ============================================
// API Client
// ============================================

const api = {
  stats: {
    get: async (): Promise<UserStats> => {
      const res = await fetch('/api/stats')
      if (!res.ok) throw new Error('Failed to fetch user statistics')
      return res.json()
    },
  },
}

// ============================================
// Query Keys Factory
// ============================================

export const statsKeys = {
  all: ['stats'] as const,
  detail: () => [...statsKeys.all, 'detail'] as const,
}

// ============================================
// Query Hook
// ============================================

/**
 * Fetch user statistics from /api/stats
 */
export function useStats() {
  return useQuery({
    queryKey: statsKeys.detail(),
    queryFn: api.stats.get,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}
