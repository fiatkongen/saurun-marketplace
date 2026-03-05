/**
 * AnalyticsPanel
 *
 * Displays analytics data: daily signups chart and session metrics.
 * Receives data as props (fetched at route level via TanStack Query).
 */

import { StatCard } from './StatCard'
import type { UserStats } from '@/types/stats'

interface AnalyticsPanelProps {
  stats: UserStats
}

export function AnalyticsPanel({ stats }: AnalyticsPanelProps) {
  const maxSignups = Math.max(...stats.dailySignups.map((d) => d.count), 1)

  return (
    <div
      role="tabpanel"
      id="tabpanel-analytics"
      aria-labelledby="dashboard-tab-analytics"
      data-testid="dashboard-panel-analytics"
    >
      <div className="grid gap-4 md:grid-cols-2">
        <StatCard
          label="Avg Session"
          value={`${stats.avgSessionMinutes} min`}
          description="Average session duration"
        />
        <StatCard
          label="Active Users"
          value={stats.activeUsers.toLocaleString()}
          description="Monthly active users"
        />
      </div>

      <div className="mt-8">
        <h3 className="text-lg font-semibold mb-4">Daily Signups</h3>
        <div
          className="rounded-lg border bg-card p-6"
          data-testid="analytics-signups-chart"
        >
          <div className="flex items-end gap-1 h-40">
            {stats.dailySignups.map((day) => (
              <div
                key={day.date}
                className="flex-1 flex flex-col items-center gap-1"
              >
                <div
                  className="w-full rounded-t bg-primary/80 transition-all hover:bg-primary"
                  style={{
                    height: `${(day.count / maxSignups) * 100}%`,
                    minHeight: day.count > 0 ? '4px' : '0px',
                  }}
                  title={`${day.date}: ${day.count} signups`}
                  data-testid={`chart-bar-${day.date}`}
                />
              </div>
            ))}
          </div>
          <div className="flex gap-1 mt-2">
            {stats.dailySignups.map((day) => (
              <div
                key={day.date}
                className="flex-1 text-center text-[10px] text-muted-foreground truncate"
              >
                {day.date.slice(5)}
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
