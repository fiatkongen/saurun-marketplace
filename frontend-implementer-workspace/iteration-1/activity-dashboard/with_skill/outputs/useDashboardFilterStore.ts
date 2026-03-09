// ============================================
// Dashboard Filter Store (Zustand)
//
// Global UI state: filter selections.
// Server data is handled by TanStack Query.
// ============================================

import { create } from "zustand"
import { useShallow } from "zustand/react/shallow"
import { devtools } from "zustand/middleware"
import type { ActivityFilters, ActivityType } from "./types"

interface DashboardFilterState extends ActivityFilters {
  setDateRange: (from: string | null, to: string | null) => void
  toggleType: (type: ActivityType) => void
  setTypes: (types: ActivityType[]) => void
  clearFilters: () => void
  reset: () => void
}

const initialState: ActivityFilters = {
  dateFrom: null,
  dateTo: null,
  types: [],
}

export const useDashboardFilterStore = create<DashboardFilterState>()(
  devtools(
    (set, get) => ({
      ...initialState,

      setDateRange: (from, to) => {
        set({ dateFrom: from, dateTo: to }, false, "setDateRange")
      },

      toggleType: (type) => {
        const current = get().types
        const next = current.includes(type)
          ? current.filter((t) => t !== type)
          : [...current, type]
        set({ types: next }, false, "toggleType")
      },

      setTypes: (types) => {
        set({ types }, false, "setTypes")
      },

      clearFilters: () => {
        set(initialState, false, "clearFilters")
      },

      reset: () => {
        set(initialState, false, "reset")
      },
    }),
    { name: "dashboard-filter-store" }
  )
)

// ============================================
// Selector hooks
// ============================================

export const useDateRange = () =>
  useDashboardFilterStore(
    useShallow((s) => ({ dateFrom: s.dateFrom, dateTo: s.dateTo }))
  )

export const useSelectedTypes = () =>
  useDashboardFilterStore((s) => s.types)

export const useFilterActions = () =>
  useDashboardFilterStore(
    useShallow((s) => ({
      setDateRange: s.setDateRange,
      toggleType: s.toggleType,
      setTypes: s.setTypes,
      clearFilters: s.clearFilters,
    }))
  )

export const useFiltersSnapshot = (): ActivityFilters =>
  useDashboardFilterStore(
    useShallow((s) => ({
      dateFrom: s.dateFrom,
      dateTo: s.dateTo,
      types: s.types,
    }))
  )
