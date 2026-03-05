/**
 * DashboardTabs
 *
 * Tab navigation for the dashboard. Active tab state lives in URL
 * (per state colocation: "In URL? -> Router params/search — selected tab").
 *
 * Receives activeTab + setter from useDashboardTab hook (lifted to page).
 */

import type { DashboardTab } from '@/types/stats'

interface DashboardTabsProps {
  activeTab: DashboardTab
  onTabChange: (tab: DashboardTab) => void
}

const tabs: { value: DashboardTab; label: string }[] = [
  { value: 'overview', label: 'Overview' },
  { value: 'analytics', label: 'Analytics' },
  { value: 'settings', label: 'Settings' },
]

export function DashboardTabs({ activeTab, onTabChange }: DashboardTabsProps) {
  return (
    <nav
      className="flex border-b"
      role="tablist"
      aria-label="Dashboard sections"
      data-testid="dashboard-tabs"
    >
      {tabs.map((tab) => (
        <button
          key={tab.value}
          role="tab"
          aria-selected={activeTab === tab.value}
          aria-controls={`tabpanel-${tab.value}`}
          onClick={() => onTabChange(tab.value)}
          className={`px-4 py-2.5 text-sm font-medium transition-colors border-b-2 -mb-px ${
            activeTab === tab.value
              ? 'border-primary text-primary'
              : 'border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground/30'
          }`}
          data-testid={`dashboard-tab-${tab.value}`}
        >
          {tab.label}
        </button>
      ))}
    </nav>
  )
}
