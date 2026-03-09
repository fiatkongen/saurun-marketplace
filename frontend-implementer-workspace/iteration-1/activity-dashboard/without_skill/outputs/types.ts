// ============================================================
// types.ts — Shared types for the Activity Dashboard
// ============================================================

export type TrendDirection = "up" | "down" | "neutral";

export interface StatCardData {
  id: string;
  label: string;
  value: string;
  trend: {
    direction: TrendDirection;
    percentage: number;
  };
  icon: string;
}

export type ActivityType = "login" | "purchase" | "signup";

export interface ActivityItem {
  id: string;
  type: ActivityType;
  user: {
    name: string;
    avatarUrl: string;
    email: string;
  };
  description: string;
  timestamp: string; // ISO 8601
  metadata?: Record<string, string>;
}

export interface ActivityFeedResponse {
  items: ActivityItem[];
  nextCursor: string | null;
  totalCount: number;
}

export interface StatsResponse {
  stats: StatCardData[];
}

export interface DateRange {
  from: Date | undefined;
  to: Date | undefined;
}

export interface DashboardFilters {
  dateRange: DateRange;
  activityTypes: ActivityType[];
}
