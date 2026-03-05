/**
 * Stats Types
 *
 * Shared types for the /api/stats endpoint response
 * and dashboard display.
 */

export interface UserStats {
  totalUsers: number
  activeUsers: number
  newUsersToday: number
  retentionRate: number
  avgSessionMinutes: number
  topCountries: CountryStat[]
  dailySignups: DailySignup[]
}

export interface CountryStat {
  country: string
  users: number
  percentage: number
}

export interface DailySignup {
  date: string
  count: number
}

export type DashboardTab = 'overview' | 'analytics' | 'settings'
