// ============================================================
// OverviewTab.tsx — Overview tab content
// ============================================================

import React from "react";
import type { UserStats } from "./types";
import { StatCard } from "./StatCard";

interface OverviewTabProps {
  stats: UserStats;
}

export function OverviewTab({ stats }: OverviewTabProps) {
  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total Users"
          value={stats.totalUsers.toLocaleString()}
        />
        <StatCard
          label="Active Users"
          value={stats.activeUsers.toLocaleString()}
          subtitle={`${((stats.activeUsers / stats.totalUsers) * 100).toFixed(1)}% of total`}
        />
        <StatCard
          label="New Today"
          value={stats.newUsersToday}
        />
        <StatCard
          label="Retention Rate"
          value={`${stats.retentionRate}%`}
        />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Top Countries
        </h3>
        <ul className="space-y-2">
          {stats.topCountries.map((c) => (
            <li key={c.country} className="flex items-center justify-between">
              <span className="text-sm text-gray-700">{c.country}</span>
              <span className="text-sm font-medium text-gray-900">
                {c.users.toLocaleString()}
              </span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}
