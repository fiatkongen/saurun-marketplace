// ============================================================
// use-dashboard-tab.ts — Sync active tab with URL search params
// ============================================================

import { useSearchParams } from "react-router-dom";
import { useCallback } from "react";
import type { DashboardTab } from "./types";

const VALID_TABS: DashboardTab[] = ["overview", "analytics", "settings"];
const DEFAULT_TAB: DashboardTab = "overview";

function isValidTab(value: string | null): value is DashboardTab {
  return VALID_TABS.includes(value as DashboardTab);
}

export function useDashboardTab() {
  const [searchParams, setSearchParams] = useSearchParams();

  const raw = searchParams.get("tab");
  const activeTab: DashboardTab = isValidTab(raw) ? raw : DEFAULT_TAB;

  const setTab = useCallback(
    (tab: DashboardTab) => {
      setSearchParams((prev) => {
        const next = new URLSearchParams(prev);
        if (tab === DEFAULT_TAB) {
          next.delete("tab");
        } else {
          next.set("tab", tab);
        }
        return next;
      });
    },
    [setSearchParams],
  );

  return { activeTab, setTab, tabs: VALID_TABS } as const;
}
