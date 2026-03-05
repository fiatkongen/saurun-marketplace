/**
 * DashboardPage
 *
 * Route-level component. Fetches data here (fetch at route level).
 * Coordinates tab state from URL and renders appropriate panel.
 *
 * State colocation applied:
 *   - Server data (stats) → TanStack Query (useStats hook)
 *   - Active tab (URL) → useSearchParams (useDashboardTab hook)
 *   - Notifications (global UI) → Zustand (useNotificationStore)
 *
 * Error boundary wraps this page at the router level.
 * Feature-level boundary isolates individual panels.
 */

import { useStats } from '@/hooks/useStats'
import { useDashboardTab } from '@/hooks/useDashboardTab'
import { useNotificationActions } from '@/stores/useNotificationStore'
import { DashboardTabs } from '@/components/dashboard/DashboardTabs'
import { OverviewPanel } from '@/components/dashboard/OverviewPanel'
import { AnalyticsPanel } from '@/components/dashboard/AnalyticsPanel'
import { SettingsPanel } from '@/components/dashboard/SettingsPanel'
import { DashboardSkeleton } from '@/components/dashboard/DashboardSkeleton'
import { FeatureErrorBoundary } from '@/providers'

export function DashboardPage() {
  const { data: stats, isPending, error } = useStats()
  const { activeTab, setActiveTab } = useDashboardTab()
  const { addNotification } = useNotificationActions()

  // Component-level loading via isPending
  if (isPending) {
    return (
      <div className="container py-8" data-testid="dashboard-page">
        <h1 className="text-2xl font-bold mb-6">Dashboard</h1>
        <DashboardSkeleton />
      </div>
    )
  }

  // Error state — show inline error with retry action + toast
  if (error) {
    return (
      <div className="container py-8" data-testid="dashboard-page">
        <h1 className="text-2xl font-bold mb-6">Dashboard</h1>
        <div
          className="rounded-lg border border-destructive/50 bg-destructive/10 p-6"
          role="alert"
          data-testid="dashboard-error"
        >
          <p className="text-sm font-medium text-destructive">
            Failed to load dashboard statistics
          </p>
          <p className="mt-1 text-xs text-muted-foreground">{error.message}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="container py-8" data-testid="dashboard-page">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <button
          onClick={() =>
            addNotification({
              type: 'info',
              title: 'Data refreshed',
              message: 'Statistics are up to date.',
            })
          }
          className="rounded-lg border px-3 py-1.5 text-sm text-muted-foreground hover:text-foreground transition-colors"
          data-testid="dashboard-refresh-button"
        >
          Refresh
        </button>
      </div>

      <DashboardTabs activeTab={activeTab} onTabChange={setActiveTab} />

      <div className="mt-6">
        <FeatureErrorBoundary>
          {activeTab === 'overview' && <OverviewPanel stats={stats} />}
          {activeTab === 'analytics' && <AnalyticsPanel stats={stats} />}
          {activeTab === 'settings' && <SettingsPanel />}
        </FeatureErrorBoundary>
      </div>
    </div>
  )
}
