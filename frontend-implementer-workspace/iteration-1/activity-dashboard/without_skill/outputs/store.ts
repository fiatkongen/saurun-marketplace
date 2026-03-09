// ============================================================
// store.ts — Zustand store for dashboard filter state
// ============================================================

import { create } from "zustand";
import type { ActivityType, DateRange } from "./types";

interface DashboardStore {
  // Filter state
  dateRange: DateRange;
  selectedActivityTypes: ActivityType[];

  // Actions
  setDateRange: (range: DateRange) => void;
  setSelectedActivityTypes: (types: ActivityType[]) => void;
  toggleActivityType: (type: ActivityType) => void;
  resetFilters: () => void;
}

const initialDateRange: DateRange = {
  from: undefined,
  to: undefined,
};

const ALL_ACTIVITY_TYPES: ActivityType[] = ["login", "purchase", "signup"];

export const useDashboardStore = create<DashboardStore>((set) => ({
  dateRange: initialDateRange,
  selectedActivityTypes: [],

  setDateRange: (range) => set({ dateRange: range }),

  setSelectedActivityTypes: (types) => set({ selectedActivityTypes: types }),

  toggleActivityType: (type) =>
    set((state) => {
      const current = state.selectedActivityTypes;
      if (current.includes(type)) {
        return { selectedActivityTypes: current.filter((t) => t !== type) };
      }
      return { selectedActivityTypes: [...current, type] };
    }),

  resetFilters: () =>
    set({
      dateRange: initialDateRange,
      selectedActivityTypes: [],
    }),
}));

export { ALL_ACTIVITY_TYPES };
