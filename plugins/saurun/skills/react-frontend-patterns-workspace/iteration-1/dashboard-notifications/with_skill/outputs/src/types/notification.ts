/**
 * Notification Types
 *
 * Types for the global notification/toast system.
 */

export type NotificationType = 'success' | 'error' | 'warning' | 'info'

export interface Notification {
  id: string
  type: NotificationType
  title: string
  message?: string
  duration?: number
}

export type AddNotificationInput = Omit<Notification, 'id'>
