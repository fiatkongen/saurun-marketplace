/**
 * TaskList Component
 *
 * Fetches tasks via TanStack Query, applies client-side filter from Zustand.
 * Handles loading and error states at the component level.
 */

import { useMemo } from 'react'
import { useTasks } from '@/hooks/useTasks'
import { useActiveFilter } from '@/stores/useTaskFilterStore'
import { TaskItem } from '@/components/tasks/TaskItem'
import type { Task, TaskFilter } from '@/types/task'

function filterTasks(tasks: Task[], filter: TaskFilter): Task[] {
  switch (filter) {
    case 'active':
      return tasks.filter((t) => !t.completed)
    case 'completed':
      return tasks.filter((t) => t.completed)
    case 'all':
    default:
      return tasks
  }
}

export function TaskList() {
  const { data: tasks, isPending, error } = useTasks()
  const filter = useActiveFilter()

  const filteredTasks = useMemo(
    () => filterTasks(tasks ?? [], filter),
    [tasks, filter]
  )

  if (isPending) {
    return <TaskListSkeleton />
  }

  if (error) {
    return (
      <div
        data-testid="task-list-error"
        className="rounded-sm border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive"
      >
        {error.message}
      </div>
    )
  }

  if (filteredTasks.length === 0) {
    return (
      <p data-testid="task-list-empty" className="py-8 text-center text-muted-foreground">
        {filter === 'all'
          ? 'No tasks yet. Create one above.'
          : `No ${filter} tasks.`}
      </p>
    )
  }

  return (
    <ul data-testid="task-list" className="flex flex-col gap-2">
      {filteredTasks.map((task) => (
        <TaskItem key={task.id} task={task} />
      ))}
    </ul>
  )
}

// ============================================
// Loading Skeleton
// ============================================

function TaskListSkeleton() {
  return (
    <div data-testid="task-list-loading" className="flex flex-col gap-2">
      {Array.from({ length: 4 }).map((_, i) => (
        <div
          key={i}
          className="h-12 animate-pulse rounded-sm bg-muted"
        />
      ))}
    </div>
  )
}
