/**
 * SettingsPanel
 *
 * Dashboard settings tab. Demonstrates notification system integration:
 * mutations trigger toasts via useNotificationActions on success/error.
 */

import { useState } from 'react'
import { useNotificationActions } from '@/stores/useNotificationStore'

export function SettingsPanel() {
  const [emailNotifications, setEmailNotifications] = useState(true)
  const [weeklyDigest, setWeeklyDigest] = useState(false)
  const { addNotification } = useNotificationActions()

  const handleSave = () => {
    // Simulate saving settings — in real app this would be a useMutation
    addNotification({
      type: 'success',
      title: 'Settings saved',
      message: 'Your dashboard preferences have been updated.',
    })
  }

  const handleReset = () => {
    setEmailNotifications(true)
    setWeeklyDigest(false)
    addNotification({
      type: 'info',
      title: 'Settings reset',
      message: 'Preferences restored to defaults.',
    })
  }

  return (
    <div
      role="tabpanel"
      id="tabpanel-settings"
      aria-labelledby="dashboard-tab-settings"
      data-testid="dashboard-panel-settings"
    >
      <div className="max-w-lg space-y-6">
        <h3 className="text-lg font-semibold">Notification Preferences</h3>

        <label
          className="flex items-center justify-between rounded-lg border p-4 cursor-pointer"
          data-testid="settings-email-toggle"
        >
          <div>
            <p className="text-sm font-medium">Email Notifications</p>
            <p className="text-xs text-muted-foreground">
              Receive email alerts for important events
            </p>
          </div>
          <input
            type="checkbox"
            checked={emailNotifications}
            onChange={(e) => setEmailNotifications(e.target.checked)}
            className="h-4 w-4 rounded border-muted-foreground"
            data-testid="settings-email-checkbox"
          />
        </label>

        <label
          className="flex items-center justify-between rounded-lg border p-4 cursor-pointer"
          data-testid="settings-digest-toggle"
        >
          <div>
            <p className="text-sm font-medium">Weekly Digest</p>
            <p className="text-xs text-muted-foreground">
              Get a weekly summary of your dashboard metrics
            </p>
          </div>
          <input
            type="checkbox"
            checked={weeklyDigest}
            onChange={(e) => setWeeklyDigest(e.target.checked)}
            className="h-4 w-4 rounded border-muted-foreground"
            data-testid="settings-digest-checkbox"
          />
        </label>

        <div className="flex gap-3 pt-2">
          <button
            onClick={handleSave}
            className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90 transition-colors"
            data-testid="settings-save-button"
          >
            Save Changes
          </button>
          <button
            onClick={handleReset}
            className="rounded-lg border px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground hover:border-foreground/30 transition-colors"
            data-testid="settings-reset-button"
          >
            Reset to Defaults
          </button>
        </div>
      </div>
    </div>
  )
}
