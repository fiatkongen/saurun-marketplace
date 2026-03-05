/**
 * TasksPage — Route-level component
 *
 * Composes feature components. Error boundary at page level
 * catches unhandled errors from child components.
 */

import { ErrorBoundary, type FallbackProps } from 'react-error-boundary'
import { CreateTaskForm } from '@/components/tasks/CreateTaskForm'
import { TaskFilters } from '@/components/tasks/TaskFilters'
import { TaskList } from '@/components/tasks/TaskList'

function PageErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="flex min-h-[400px] flex-col items-center justify-center gap-4 p-4">
      <h2 className="text-xl font-bold text-destructive">Something went wrong</h2>
      <p className="text-muted-foreground">{error.message}</p>
      <button
        data-testid="task-page-error-retry"
        onClick={resetErrorBoundary}
        className="rounded-sm bg-primary px-4 py-2 text-sm text-primary-foreground hover:bg-primary/90"
      >
        Try again
      </button>
    </div>
  )
}

export function TasksPage() {
  return (
    <ErrorBoundary FallbackComponent={PageErrorFallback}>
      <div className="mx-auto max-w-2xl px-4 py-8">
        <h1 className="mb-6 text-2xl font-bold">Tasks</h1>

        <div className="mb-4">
          <CreateTaskForm />
        </div>

        <div className="mb-4 flex items-center justify-between">
          <TaskFilters />
        </div>

        <TaskList />
      </div>
    </ErrorBoundary>
  )
}
