/**
 * Notification Store
 *
 * Zustand store for global UI notification state.
 * One store per feature domain. Actions own async logic. Reset for tests.
 *
 * Notifications are Global UI state → Zustand store (per state colocation rules).
 */

import { create } from 'zustand'
import { useShallow } from 'zustand/react/shallow'
import { devtools } from 'zustand/middleware'
import type { Notification, AddNotificationInput } from '@/types/notification'

// ============================================
// Types
// ============================================

interface NotificationState {
  // State
  notifications: Notification[]

  // Actions
  addNotification: (input: AddNotificationInput) => void
  dismissNotification: (id: string) => void
  clearAll: () => void
  reset: () => void
}

// ============================================
// Initial State (for reset)
// ============================================

const initialState = {
  notifications: [] as Notification[],
}

// ============================================
// Helpers
// ============================================

let nextId = 0
function generateId(): string {
  return `notification-${Date.now()}-${nextId++}`
}

const DEFAULT_DURATION = 5000

// ============================================
// Store
// ============================================

export const useNotificationStore = create<NotificationState>()(
  devtools(
    (set) => ({
      ...initialState,

      addNotification: (input) => {
        const id = generateId()
        const notification: Notification = { ...input, id }

        set(
          (state) => ({
            notifications: [...state.notifications, notification],
          }),
          false,
          'addNotification'
        )

        // Auto-dismiss after duration
        const duration = input.duration ?? DEFAULT_DURATION
        if (duration > 0) {
          setTimeout(() => {
            set(
              (state) => ({
                notifications: state.notifications.filter((n) => n.id !== id),
              }),
              false,
              'autoDismiss'
            )
          }, duration)
        }
      },

      dismissNotification: (id) => {
        set(
          (state) => ({
            notifications: state.notifications.filter((n) => n.id !== id),
          }),
          false,
          'dismissNotification'
        )
      },

      clearAll: () => {
        set({ notifications: [] }, false, 'clearAll')
      },

      reset: () => {
        set(initialState, false, 'reset')
      },
    }),
    { name: 'notification-store' }
  )
)

// ============================================
// Selector Hooks (use these in components)
// ============================================

// Single field selectors
export const useNotifications = () =>
  useNotificationStore((s) => s.notifications)

// Derived state (computed in selector, NOT stored)
export const useNotificationCount = () =>
  useNotificationStore((s) => s.notifications.length)

// Action selectors
export const useNotificationActions = () =>
  useNotificationStore(
    useShallow((s) => ({
      addNotification: s.addNotification,
      dismissNotification: s.dismissNotification,
      clearAll: s.clearAll,
    }))
  )
