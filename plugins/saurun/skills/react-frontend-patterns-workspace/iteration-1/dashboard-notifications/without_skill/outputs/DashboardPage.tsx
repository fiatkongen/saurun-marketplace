// ============================================================
// DashboardPage.tsx — Main dashboard page with tabbed layout
// ============================================================

import React from "react";
import { useStats } from "./use-stats";
import { useDashboardTab } from "./use-dashboard-tab";
import { toast } from "./toast-store";
import { OverviewTab } from "./OverviewTab";
import { AnalyticsTab } from "./AnalyticsTab";
import { SettingsTab } from "./SettingsTab";
import type { DashboardTab } from "./types";

const tabLabels: Record<DashboardTab, string> = {
  overview: "Overview",
  analytics: "Analytics",
  settings: "Settings",
};

export function DashboardPage() {
  const { stats, isLoading, error, refetch } = useStats();
  const { activeTab, setTab, tabs } = useDashboardTab();

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="mx-auto max-w-6xl px-4 py-8">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          <button
            onClick={() => {
              refetch();
              toast.info("Refreshing statistics...");
            }}
            className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 transition-colors"
          >
            Refresh
          </button>
        </div>

        {/* Tabs */}
        <div className="border-b border-gray-200 mb-6">
          <nav className="flex gap-6" aria-label="Dashboard tabs">
            {tabs.map((tab) => (
              <button
                key={tab}
                onClick={() => setTab(tab)}
                className={`pb-3 text-sm font-medium transition-colors border-b-2 ${
                  activeTab === tab
                    ? "border-blue-600 text-blue-600"
                    : "border-transparent text-gray-500 hover:text-gray-700"
                }`}
                aria-current={activeTab === tab ? "page" : undefined}
              >
                {tabLabels[tab]}
              </button>
            ))}
          </nav>
        </div>

        {/* Content */}
        {isLoading && (
          <div className="flex items-center justify-center py-20">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-gray-200 border-t-blue-600" />
          </div>
        )}

        {error && !isLoading && (
          <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
            {error}
            <button
              onClick={refetch}
              className="ml-2 underline hover:no-underline"
            >
              Retry
            </button>
          </div>
        )}

        {stats && !isLoading && (
          <>
            {activeTab === "overview" && <OverviewTab stats={stats} />}
            {activeTab === "analytics" && <AnalyticsTab stats={stats} />}
            {activeTab === "settings" && <SettingsTab />}
          </>
        )}
      </div>
    </div>
  );
}
