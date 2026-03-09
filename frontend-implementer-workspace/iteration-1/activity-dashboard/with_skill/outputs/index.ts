// ============================================
// Public API — Activity Dashboard
// ============================================

// Page
export { DashboardPage } from "./DashboardPage"

// Components
export { StatCard, StatCardSkeleton } from "./StatCard"
export { StatsRow } from "./StatsRow"
export { ActivityFeed } from "./ActivityFeed"
export { ActivityFeedItem, ActivityFeedItemSkeleton } from "./ActivityFeedItem"
export { ActivityIcon } from "./ActivityIcon"
export { UserAvatar } from "./UserAvatar"
export { FilterBar } from "./FilterBar"

// Hooks
export { useDashboardStats, useActivityFeed, dashboardKeys } from "./useDashboardQueries"

// Store
export {
  useDashboardFilterStore,
  useDateRange,
  useSelectedTypes,
  useFilterActions,
  useFiltersSnapshot,
} from "./useDashboardFilterStore"

// Types
export type {
  ActivityType,
  TrendDirection,
  StatCard as StatCardType,
  ActivityItem,
  ActivityFeedResponse,
  DashboardStatsResponse,
  ActivityFilters,
  UserAvatar as UserAvatarType,
} from "./types"

// Utilities
export { formatRelativeTime } from "./formatRelativeTime"
export { cn } from "./cn"
