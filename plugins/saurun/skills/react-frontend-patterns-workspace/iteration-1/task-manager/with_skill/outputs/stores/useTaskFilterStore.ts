/**
 * Task Filter Store (Zustand)
 *
 * Global UI state only — the active filter selection.
 * Server data (tasks) lives in TanStack Query, NOT here.
 * One store per feature domain. Always use selectors.
 */

import { create } from 'zustand'
import { devtools } from 'zustand/middleware'
import type { TaskFilter } from '@/types/task'

// ============================================
// Types
// ============================================

interface TaskFilterState {
  // State
  filter: TaskFilter

  // Actions
  setFilter: (filter: TaskFilter) => void
  reset: () => void
}

// ============================================
// Initial State (for reset)
// ============================================

const initialState = {
  filter: 'all' as TaskFilter,
}

// ============================================
// Store
// ============================================

export const useTaskFilterStore = create<TaskFilterState>()(
  devtools(
    (set) => ({
      ...initialState,

      setFilter: (filter) => {
        set({ filter }, false, 'setFilter')
      },

      reset: () => {
        set(initialState, false, 'reset')
      },
    }),
    { name: 'task-filter-store' }
  )
)

// ============================================
// Selectors (use these in components)
// ============================================

export const useActiveFilter = () => useTaskFilterStore((s) => s.filter)
export const useSetFilter = () => useTaskFilterStore((s) => s.setFilter)
