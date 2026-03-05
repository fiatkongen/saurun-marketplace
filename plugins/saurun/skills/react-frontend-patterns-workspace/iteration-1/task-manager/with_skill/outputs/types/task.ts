/**
 * Task domain types
 *
 * Shared across hooks, stores, and components.
 */

export interface Task {
  id: string
  title: string
  completed: boolean
  createdAt: string
}

export interface CreateTaskInput {
  title: string
}

export type TaskFilter = 'all' | 'active' | 'completed'
