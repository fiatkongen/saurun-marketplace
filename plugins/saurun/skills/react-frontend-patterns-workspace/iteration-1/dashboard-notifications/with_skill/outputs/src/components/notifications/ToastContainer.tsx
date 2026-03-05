/**
 * ToastContainer
 *
 * Renders all active notifications as toast cards.
 * Mounted once at the app root (inside Providers).
 *
 * Uses selector hooks — never bare useStore().
 */

import { useNotifications, useNotificationActions } from '@/stores/useNotificationStore'
import type { NotificationType } from '@/types/notification'

// ============================================
// Variant Styles
// ============================================

const variantStyles: Record<NotificationType, string> = {
  success: 'border-green-500/50 bg-green-50 text-green-900 dark:bg-green-950 dark:text-green-100',
  error: 'border-red-500/50 bg-red-50 text-red-900 dark:bg-red-950 dark:text-red-100',
  warning: 'border-yellow-500/50 bg-yellow-50 text-yellow-900 dark:bg-yellow-950 dark:text-yellow-100',
  info: 'border-blue-500/50 bg-blue-50 text-blue-900 dark:bg-blue-950 dark:text-blue-100',
}

const iconMap: Record<NotificationType, string> = {
  success: 'check-circle',
  error: 'x-circle',
  warning: 'alert-triangle',
  info: 'info',
}

// ============================================
// Component
// ============================================

export function ToastContainer() {
  const notifications = useNotifications()
  const { dismissNotification } = useNotificationActions()

  if (notifications.length === 0) return null

  return (
    <div
      className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 w-80"
      aria-live="polite"
      aria-label="Notifications"
      data-testid="toast-container"
    >
      {notifications.map((notification) => (
        <div
          key={notification.id}
          className={`rounded-lg border p-4 shadow-lg transition-all animate-in slide-in-from-right-full ${variantStyles[notification.type]}`}
          role="alert"
          data-testid={`toast-${notification.id}`}
        >
          <div className="flex items-start gap-3">
            <span className="text-sm font-medium" aria-hidden="true">
              {iconMap[notification.type]}
            </span>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-semibold">{notification.title}</p>
              {notification.message && (
                <p className="mt-1 text-xs opacity-80">{notification.message}</p>
              )}
            </div>
            <button
              onClick={() => dismissNotification(notification.id)}
              className="shrink-0 rounded-sm p-0.5 opacity-60 hover:opacity-100 transition-opacity"
              aria-label={`Dismiss ${notification.title}`}
              data-testid={`toast-dismiss-${notification.id}`}
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="14"
                height="14"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              >
                <path d="M18 6 6 18" />
                <path d="m6 6 12 12" />
              </svg>
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
