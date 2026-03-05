/**
 * TaskItem Component
 *
 * Renders a single task with a toggle for completion.
 * Uses mutation hook for toggle — loading state on the button.
 */

import type { Task } from '@/types/task'
import { useToggleTask } from '@/hooks/useTasks'

interface TaskItemProps {
  task: Task
}

export function TaskItem({ task }: TaskItemProps) {
  const toggleTask = useToggleTask()

  return (
    <li
      data-testid={`task-item-${task.id}`}
      className="flex items-center gap-3 rounded-sm border px-4 py-3"
    >
      <input
        type="checkbox"
        data-testid={`task-item-${task.id}-toggle`}
        checked={task.completed}
        disabled={toggleTask.isPending}
        onChange={() =>
          toggleTask.mutate({ id: task.id, completed: !task.completed })
        }
        className="h-4 w-4 rounded border-input accent-primary"
      />
      <span
        className={
          task.completed
            ? 'flex-1 text-muted-foreground line-through'
            : 'flex-1'
        }
      >
        {task.title}
      </span>
    </li>
  )
}
