/**
 * CreateTaskForm Component
 *
 * Simple inline form for creating new tasks.
 * Uses mutation hook — button disabled while pending.
 * Local form state via useState (form input = local to component).
 */

import { useState, type FormEvent } from 'react'
import { useCreateTask } from '@/hooks/useTasks'

export function CreateTaskForm() {
  const [title, setTitle] = useState('')
  const createTask = useCreateTask()

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const trimmed = title.trim()
    if (!trimmed) return

    createTask.mutate(
      { title: trimmed },
      {
        onSuccess: () => setTitle(''),
      }
    )
  }

  return (
    <form
      data-testid="create-task-form"
      onSubmit={handleSubmit}
      className="flex gap-2"
    >
      <input
        data-testid="create-task-form-title"
        type="text"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        placeholder="What needs to be done?"
        disabled={createTask.isPending}
        className="flex-1 rounded-sm border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
      />
      <button
        data-testid="create-task-form-submit"
        type="submit"
        disabled={createTask.isPending || !title.trim()}
        className="rounded-sm bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
      >
        {createTask.isPending ? 'Adding...' : 'Add Task'}
      </button>
    </form>
  )
}
