// ============================================
// Domain Types
// ============================================

export type ActivityType = "login" | "purchase" | "signup"

export type TrendDirection = "up" | "down" | "flat"

export interface StatCard {
  id: string
  label: string
  value: string
  trend: TrendDirection
  trendPercent: number
  prefix?: string
}

export interface UserAvatar {
  name: string
  imageUrl?: string
}

export interface ActivityItem {
  id: string
  type: ActivityType
  user: UserAvatar
  description: string
  timestamp: string // ISO 8601
}

export interface ActivityFeedResponse {
  items: ActivityItem[]
  nextCursor: string | null
  hasMore: boolean
}

export interface DashboardStatsResponse {
  stats: StatCard[]
}

export interface ActivityFilters {
  dateFrom: string | null
  dateTo: string | null
  types: ActivityType[]
}
