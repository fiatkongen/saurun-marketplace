// ============================================
// Settings Page Types
// ============================================

export type SettingsTab = 'profile' | 'notifications' | 'appearance'

// --- Profile ---

export interface ProfileData {
  name: string
  email: string
  bio: string
  avatarUrl: string | null
}

// --- Notifications ---

export type NotificationChannel = 'email' | 'push' | 'sms'

export interface NotificationCategory {
  id: string
  label: string
  description: string
  channels: Record<NotificationChannel, boolean>
}

export interface NotificationsData {
  categories: NotificationCategory[]
}

// --- Appearance ---

export type ThemeMode = 'light' | 'dark' | 'system'

export interface AppearanceData {
  theme: ThemeMode
  fontSize: number // 12-20
  accentColor: string // hex color
}

// --- Combined Settings ---

export interface SettingsState {
  profile: ProfileData
  notifications: NotificationsData
  appearance: AppearanceData
}
