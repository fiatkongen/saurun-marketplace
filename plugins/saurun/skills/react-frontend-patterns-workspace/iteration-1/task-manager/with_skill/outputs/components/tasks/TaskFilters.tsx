/**
 * TaskFilters Component
 *
 * Filter buttons for all/active/completed.
 * Reads and writes to Zustand filter store via selector hooks.
 */

import { useActiveFilter, useSetFilter } from '@/stores/useTaskFilterStore'
import type { TaskFilter } from '@/types/task'

const FILTERS: { value: TaskFilter; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'active', label: 'Active' },
  { value: 'completed', label: 'Completed' },
]

export function TaskFilters() {
  const activeFilter = useActiveFilter()
  const setFilter = useSetFilter()

  return (
    <div data-testid="task-filters" className="flex gap-1" role="group" aria-label="Filter tasks">
      {FILTERS.map(({ value, label }) => (
        <button
          key={value}
          data-testid={`task-filters-${value}`}
          onClick={() => setFilter(value)}
          className={
            activeFilter === value
              ? 'rounded-sm bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground'
              : 'rounded-sm px-3 py-1.5 text-sm text-muted-foreground hover:bg-muted'
          }
        >
          {label}
        </button>
      ))}
    </div>
  )
}
