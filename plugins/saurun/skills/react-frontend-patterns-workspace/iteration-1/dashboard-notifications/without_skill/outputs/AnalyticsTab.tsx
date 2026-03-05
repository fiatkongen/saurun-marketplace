// ============================================================
// AnalyticsTab.tsx — Analytics tab content
// ============================================================

import React from "react";
import type { UserStats } from "./types";
import { StatCard } from "./StatCard";

interface AnalyticsTabProps {
  stats: UserStats;
}

export function AnalyticsTab({ stats }: AnalyticsTabProps) {
  const maxSignups = Math.max(...stats.dailySignups.map((d) => d.count));

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <StatCard
          label="Avg. Session"
          value={`${stats.avgSessionMinutes} min`}
        />
        <StatCard
          label="Retention"
          value={`${stats.retentionRate}%`}
          subtitle="30-day rolling"
        />
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">
          Daily Signups (Last 7 days)
        </h3>
        <div className="space-y-2">
          {stats.dailySignups.map((day) => (
            <div key={day.date} className="flex items-center gap-3">
              <span className="w-20 shrink-0 text-xs text-gray-500">
                {day.date}
              </span>
              <div className="flex-1 h-5 bg-gray-100 rounded overflow-hidden">
                <div
                  className="h-full bg-blue-500 rounded transition-all"
                  style={{
                    width: `${(day.count / maxSignups) * 100}%`,
                  }}
                />
              </div>
              <span className="w-10 text-right text-xs font-medium text-gray-700">
                {day.count}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
