import { cn } from './cn'
import {
  useNotificationCategories,
  useToggleNotification,
} from './useSettingsStore'
import type { NotificationChannel } from './types'

const CHANNELS: { key: NotificationChannel; label: string; icon: React.ReactNode }[] = [
  {
    key: 'email',
    label: 'Email',
    icon: (
      <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
        <rect x="1.5" y="3" width="13" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.2" />
        <path d="M2 4l6 4.5L14 4" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    ),
  },
  {
    key: 'push',
    label: 'Push',
    icon: (
      <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
        <path d="M5 12.5a3 3 0 006 0M8 1a5 5 0 00-5 5c0 2.5-1.5 4-1.5 4h13S13 8.5 13 6a5 5 0 00-5-5z" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    ),
  },
  {
    key: 'sms',
    label: 'SMS',
    icon: (
      <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
        <rect x="3.5" y="1" width="9" height="14" rx="2" stroke="currentColor" strokeWidth="1.2" />
        <line x1="6" y1="12.5" x2="10" y2="12.5" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" />
      </svg>
    ),
  },
]

interface ToggleSwitchProps {
  checked: boolean
  onChange: () => void
  label: string
  testId: string
}

function ToggleSwitch({ checked, onChange, label, testId }: ToggleSwitchProps) {
  return (
    <button
      data-testid={testId}
      role="switch"
      aria-checked={checked}
      aria-label={label}
      onClick={onChange}
      className={cn(
        'relative inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full',
        'transition-colors duration-200 ease-in-out',
        'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-zinc-500',
        checked
          ? 'bg-zinc-900 dark:bg-zinc-100'
          : 'bg-zinc-200 dark:bg-zinc-700'
      )}
    >
      <span
        className={cn(
          'pointer-events-none inline-block h-4.5 w-4.5 rounded-full shadow-xs',
          'transform transition-transform duration-200 ease-in-out',
          checked
            ? 'translate-x-5.5 bg-white dark:bg-zinc-900'
            : 'translate-x-0.5 bg-white dark:bg-zinc-400'
        )}
      />
    </button>
  )
}

export function NotificationsTab() {
  const categories = useNotificationCategories()
  const toggleChannel = useToggleNotification()

  return (
    <div className="space-y-8" data-testid="settings-notifications-tab">
      {/* Section header */}
      <div>
        <h2 className="text-lg font-semibold text-zinc-900 dark:text-zinc-100">
          Notifications
        </h2>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Choose how and when you want to be notified.
        </p>
      </div>

      {/* Channel legend (desktop) */}
      <div className="hidden sm:grid sm:grid-cols-[1fr_auto] sm:items-end sm:gap-4">
        <div />
        <div className="flex items-center gap-6 pr-1">
          {CHANNELS.map((ch) => (
            <div
              key={ch.key}
              className="flex w-14 flex-col items-center gap-1 text-xs text-zinc-500 dark:text-zinc-400"
            >
              {ch.icon}
              <span>{ch.label}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Categories */}
      <div className="divide-y divide-zinc-100 dark:divide-zinc-800">
        {categories.map((category) => (
          <div
            key={category.id}
            data-testid={`settings-notification-category-${category.id}`}
            className="py-5 first:pt-0 last:pb-0"
          >
            <div className="sm:grid sm:grid-cols-[1fr_auto] sm:items-start sm:gap-4">
              {/* Category info */}
              <div className="mb-4 sm:mb-0">
                <h3 className="text-sm font-medium text-zinc-900 dark:text-zinc-100">
                  {category.label}
                </h3>
                <p className="mt-0.5 text-sm text-zinc-500 dark:text-zinc-400">
                  {category.description}
                </p>
              </div>

              {/* Channel toggles */}
              <div className="flex items-center gap-6 pr-1">
                {CHANNELS.map((ch) => (
                  <div
                    key={ch.key}
                    className="flex w-14 flex-col items-center gap-1.5"
                  >
                    {/* Mobile label */}
                    <span className="text-xs text-zinc-400 sm:hidden">
                      {ch.label}
                    </span>
                    <ToggleSwitch
                      checked={category.channels[ch.key]}
                      onChange={() =>
                        toggleChannel(category.id, ch.key)
                      }
                      label={`${ch.label} notifications for ${category.label}`}
                      testId={`settings-notification-${category.id}-${ch.key}`}
                    />
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
