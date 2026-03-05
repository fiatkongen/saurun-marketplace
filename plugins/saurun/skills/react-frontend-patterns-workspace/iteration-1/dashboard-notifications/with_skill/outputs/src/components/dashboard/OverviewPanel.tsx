/**
 * OverviewPanel
 *
 * Displays key stat cards and top countries.
 * Receives data as props (fetched at route level via TanStack Query).
 */

import { StatCard } from './StatCard'
import type { UserStats } from '@/types/stats'

interface OverviewPanelProps {
  stats: UserStats
}

export function OverviewPanel({ stats }: OverviewPanelProps) {
  return (
    <div
      role="tabpanel"
      id="tabpanel-overview"
      aria-labelledby="dashboard-tab-overview"
      data-testid="dashboard-panel-overview"
    >
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total Users"
          value={stats.totalUsers.toLocaleString()}
          description="All registered accounts"
        />
        <StatCard
          label="Active Users"
          value={stats.activeUsers.toLocaleString()}
          description="Active in the last 30 days"
          trend={{ value: 12, isPositive: true }}
        />
        <StatCard
          label="New Today"
          value={stats.newUsersToday}
          description="Signups in the last 24h"
        />
        <StatCard
          label="Retention Rate"
          value={`${stats.retentionRate}%`}
          description="30-day retention"
          trend={{ value: 2.1, isPositive: true }}
        />
      </div>

      <div className="mt-8">
        <h3 className="text-lg font-semibold mb-4">Top Countries</h3>
        <div className="rounded-lg border bg-card">
          <table className="w-full" data-testid="overview-countries-table">
            <thead>
              <tr className="border-b text-left text-sm text-muted-foreground">
                <th className="p-3 font-medium">Country</th>
                <th className="p-3 font-medium text-right">Users</th>
                <th className="p-3 font-medium text-right">Share</th>
              </tr>
            </thead>
            <tbody>
              {stats.topCountries.map((country) => (
                <tr
                  key={country.country}
                  className="border-b last:border-b-0"
                  data-testid={`country-row-${country.country.toLowerCase()}`}
                >
                  <td className="p-3 text-sm">{country.country}</td>
                  <td className="p-3 text-sm text-right">
                    {country.users.toLocaleString()}
                  </td>
                  <td className="p-3 text-sm text-right">
                    {country.percentage}%
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
