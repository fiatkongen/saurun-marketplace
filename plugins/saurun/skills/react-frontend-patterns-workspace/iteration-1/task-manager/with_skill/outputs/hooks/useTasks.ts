/**
 * TanStack Query hooks for tasks
 *
 * Server data lives in TanStack Query cache — never Zustand.
 * Query keys follow [domain, action, params] pattern.
 * Mutations invalidate relevant queries on success.
 */

import {
  useQuery,
  useMutation,
  useQueryClient,
} from '@tanstack/react-query'
import type { Task, CreateTaskInput } from '@/types/task'

// ============================================
// API Client
// ============================================

const api = {
  tasks: {
    list: async (): Promise<Task[]> => {
      const res = await fetch('/api/tasks')
      if (!res.ok) throw new Error('Failed to fetch tasks')
      return res.json()
    },

    create: async (input: CreateTaskInput): Promise<Task> => {
      const res = await fetch('/api/tasks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(input),
      })
      if (!res.ok) throw new Error('Failed to create task')
      return res.json()
    },

    update: async ({
      id,
      ...input
    }: {
      id: string
      completed: boolean
    }): Promise<Task> => {
      const res = await fetch(`/api/tasks/${id}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(input),
      })
      if (!res.ok) throw new Error('Failed to update task')
      return res.json()
    },
  },
}

// ============================================
// Query Keys Factory
// ============================================

export const taskKeys = {
  all: ['tasks'] as const,
  lists: () => [...taskKeys.all, 'list'] as const,
  list: () => [...taskKeys.lists()] as const,
  details: () => [...taskKeys.all, 'detail'] as const,
  detail: (id: string) => [...taskKeys.details(), id] as const,
}

// ============================================
// Query Hooks
// ============================================

/**
 * Fetch all tasks
 */
export function useTasks() {
  return useQuery({
    queryKey: taskKeys.list(),
    queryFn: api.tasks.list,
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Create a new task
 */
export function useCreateTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.tasks.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
    },
    onError: (error) => {
      console.error('Failed to create task:', error)
    },
  })
}

/**
 * Toggle task completion status
 */
export function useToggleTask() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.tasks.update,
    onSuccess: (data) => {
      // Update the specific task in cache
      queryClient.setQueryData(taskKeys.detail(data.id), data)
      // Invalidate lists to refetch
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
    },
    onError: (error) => {
      console.error('Failed to update task:', error)
    },
  })
}
