/**
 * Dashboard Tab Hook
 *
 * Active tab state lives in URL search params (per state colocation rules:
 * "In URL? → Router params/search — filters, pagination, selected tab").
 *
 * Uses useSearchParams to read/write the ?tab= query parameter.
 */

import { useSearchParams } from 'react-router-dom'
import type { DashboardTab } from '@/types/stats'

const VALID_TABS: DashboardTab[] = ['overview', 'analytics', 'settings']
const DEFAULT_TAB: DashboardTab = 'overview'

function isValidTab(value: string | null): value is DashboardTab {
  return value !== null && VALID_TABS.includes(value as DashboardTab)
}

/**
 * Read and set the active dashboard tab from URL search params.
 * Falls back to 'overview' if param is missing or invalid.
 */
export function useDashboardTab() {
  const [searchParams, setSearchParams] = useSearchParams()

  const rawTab = searchParams.get('tab')
  const activeTab: DashboardTab = isValidTab(rawTab) ? rawTab : DEFAULT_TAB

  const setActiveTab = (tab: DashboardTab) => {
    setSearchParams(
      (prev) => {
        const next = new URLSearchParams(prev)
        if (tab === DEFAULT_TAB) {
          next.delete('tab')
        } else {
          next.set('tab', tab)
        }
        return next
      },
      { replace: true }
    )
  }

  return { activeTab, setActiveTab } as const
}
