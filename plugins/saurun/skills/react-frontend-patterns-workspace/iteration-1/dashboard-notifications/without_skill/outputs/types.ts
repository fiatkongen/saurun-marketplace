// ============================================================
// types.ts — Shared type definitions
// ============================================================

// --- API Types ---

export interface UserStats {
  totalUsers: number;
  activeUsers: number;
  newUsersToday: number;
  retentionRate: number;
  avgSessionMinutes: number;
  topCountries: { country: string; users: number }[];
  dailySignups: { date: string; count: number }[];
}

// --- Notification Types ---

export type ToastVariant = "success" | "error" | "warning" | "info";

export interface Toast {
  id: string;
  message: string;
  variant: ToastVariant;
  duration?: number;
}

// --- Tab Types ---

export type DashboardTab = "overview" | "analytics" | "settings";
