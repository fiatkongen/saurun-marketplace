// ============================================================
// index.ts — Public API barrel export
// ============================================================

// Page
export { DashboardPage } from "./components/DashboardPage";

// Components (for individual use / testing)
export { StatCard } from "./components/StatCard";
export { StatCardSkeleton } from "./components/StatCardSkeleton";
export { StatCardsRow } from "./components/StatCardsRow";
export { ActivityFeed } from "./components/ActivityFeed";
export { ActivityFeedItem } from "./components/ActivityFeedItem";
export { ActivityFeedSkeleton } from "./components/ActivityFeedSkeleton";
export { FilterBar } from "./components/FilterBar";

// Hooks
export { useStatsQuery, useActivityFeedQuery, useFlatActivityItems, formatRelativeTime } from "./hooks";

// Store
export { useDashboardStore } from "./store";

// Types
export type {
  StatCardData,
  TrendDirection,
  ActivityItem,
  ActivityType,
  ActivityFeedResponse,
  StatsResponse,
  DateRange,
  DashboardFilters,
} from "./types";
