/**
 * Settings Store
 *
 * Manages local form state for the settings page.
 * Tracks dirty state by comparing current values against saved snapshot.
 * Uses Zustand with devtools. Selectors exported for components.
 */

import { create } from 'zustand'
import { useShallow } from 'zustand/react/shallow'
import { devtools } from 'zustand/middleware'
import type {
  ProfileData,
  NotificationsData,
  AppearanceData,
  NotificationChannel,
  ThemeMode,
  SettingsTab,
} from './types'

// ============================================
// Default Data
// ============================================

const defaultProfile: ProfileData = {
  name: '',
  email: '',
  bio: '',
  avatarUrl: null,
}

const defaultNotifications: NotificationsData = {
  categories: [
    {
      id: 'account',
      label: 'Account Activity',
      description: 'Sign-ins, password changes, and security alerts',
      channels: { email: true, push: true, sms: false },
    },
    {
      id: 'marketing',
      label: 'Marketing',
      description: 'Product updates, newsletters, and promotions',
      channels: { email: true, push: false, sms: false },
    },
    {
      id: 'social',
      label: 'Social',
      description: 'Comments, mentions, and follows',
      channels: { email: false, push: true, sms: false },
    },
    {
      id: 'orders',
      label: 'Orders & Transactions',
      description: 'Order confirmations, shipping updates, and receipts',
      channels: { email: true, push: true, sms: true },
    },
  ],
}

const defaultAppearance: AppearanceData = {
  theme: 'system',
  fontSize: 16,
  accentColor: '#6366f1',
}

// ============================================
// Types
// ============================================

interface SettingsStoreState {
  // Current (potentially edited) values
  profile: ProfileData
  notifications: NotificationsData
  appearance: AppearanceData

  // Saved snapshot (last persisted values)
  savedProfile: ProfileData
  savedNotifications: NotificationsData
  savedAppearance: AppearanceData

  // Active tab
  activeTab: SettingsTab

  // Saving state
  isSaving: boolean

  // Toast
  toast: { message: string; type: 'success' | 'error' } | null

  // Actions
  setActiveTab: (tab: SettingsTab) => void

  // Profile actions
  setProfileField: <K extends keyof ProfileData>(field: K, value: ProfileData[K]) => void

  // Notification actions
  toggleNotificationChannel: (
    categoryId: string,
    channel: NotificationChannel
  ) => void

  // Appearance actions
  setTheme: (theme: ThemeMode) => void
  setFontSize: (size: number) => void
  setAccentColor: (color: string) => void

  // Save / discard
  save: () => Promise<void>
  discard: () => void
  showToast: (message: string, type: 'success' | 'error') => void
  dismissToast: () => void

  // Reset (for tests)
  reset: () => void
}

// ============================================
// Initial State
// ============================================

const initialState = {
  profile: { ...defaultProfile },
  notifications: {
    categories: defaultNotifications.categories.map((c) => ({
      ...c,
      channels: { ...c.channels },
    })),
  },
  appearance: { ...defaultAppearance },

  savedProfile: { ...defaultProfile },
  savedNotifications: {
    categories: defaultNotifications.categories.map((c) => ({
      ...c,
      channels: { ...c.channels },
    })),
  },
  savedAppearance: { ...defaultAppearance },

  activeTab: 'profile' as SettingsTab,
  isSaving: false,
  toast: null as { message: string; type: 'success' | 'error' } | null,
}

// ============================================
// Store
// ============================================

export const useSettingsStore = create<SettingsStoreState>()(
  devtools(
    (set, get) => ({
      ...initialState,

      setActiveTab: (tab) => {
        set({ activeTab: tab }, false, 'setActiveTab')
      },

      // --- Profile ---

      setProfileField: (field, value) => {
        set(
          (state) => ({
            profile: { ...state.profile, [field]: value },
          }),
          false,
          `setProfileField/${String(field)}`
        )
      },

      // --- Notifications ---

      toggleNotificationChannel: (categoryId, channel) => {
        set(
          (state) => ({
            notifications: {
              categories: state.notifications.categories.map((cat) =>
                cat.id === categoryId
                  ? {
                      ...cat,
                      channels: {
                        ...cat.channels,
                        [channel]: !cat.channels[channel],
                      },
                    }
                  : cat
              ),
            },
          }),
          false,
          `toggleNotification/${categoryId}/${channel}`
        )
      },

      // --- Appearance ---

      setTheme: (theme) => {
        set(
          (state) => ({
            appearance: { ...state.appearance, theme },
          }),
          false,
          'setTheme'
        )
      },

      setFontSize: (fontSize) => {
        set(
          (state) => ({
            appearance: { ...state.appearance, fontSize },
          }),
          false,
          'setFontSize'
        )
      },

      setAccentColor: (accentColor) => {
        set(
          (state) => ({
            appearance: { ...state.appearance, accentColor },
          }),
          false,
          'setAccentColor'
        )
      },

      // --- Save / Discard ---

      save: async () => {
        set({ isSaving: true }, false, 'save/start')

        // Simulate API call
        await new Promise((resolve) => setTimeout(resolve, 800))

        const state = get()
        set(
          {
            savedProfile: { ...state.profile },
            savedNotifications: {
              categories: state.notifications.categories.map((c) => ({
                ...c,
                channels: { ...c.channels },
              })),
            },
            savedAppearance: { ...state.appearance },
            isSaving: false,
          },
          false,
          'save/complete'
        )

        get().showToast('Settings saved successfully', 'success')
      },

      discard: () => {
        const state = get()
        set(
          {
            profile: { ...state.savedProfile },
            notifications: {
              categories: state.savedNotifications.categories.map((c) => ({
                ...c,
                channels: { ...c.channels },
              })),
            },
            appearance: { ...state.savedAppearance },
          },
          false,
          'discard'
        )
      },

      showToast: (message, type) => {
        set({ toast: { message, type } }, false, 'showToast')
        setTimeout(() => {
          get().dismissToast()
        }, 4000)
      },

      dismissToast: () => {
        set({ toast: null }, false, 'dismissToast')
      },

      reset: () => {
        set(
          {
            ...initialState,
            // Deep clone arrays/objects
            profile: { ...defaultProfile },
            notifications: {
              categories: defaultNotifications.categories.map((c) => ({
                ...c,
                channels: { ...c.channels },
              })),
            },
            appearance: { ...defaultAppearance },
            savedProfile: { ...defaultProfile },
            savedNotifications: {
              categories: defaultNotifications.categories.map((c) => ({
                ...c,
                channels: { ...c.channels },
              })),
            },
            savedAppearance: { ...defaultAppearance },
          },
          false,
          'reset'
        )
      },
    }),
    { name: 'settings-store' }
  )
)

// ============================================
// Selectors
// ============================================

// Tab
export const useActiveTab = () => useSettingsStore((s) => s.activeTab)
export const useSetActiveTab = () => useSettingsStore((s) => s.setActiveTab)

// Profile
export const useProfile = () =>
  useSettingsStore(
    useShallow((s) => s.profile)
  )
export const useProfileActions = () =>
  useSettingsStore((s) => s.setProfileField)

// Notifications
export const useNotificationCategories = () =>
  useSettingsStore((s) => s.notifications.categories)
export const useToggleNotification = () =>
  useSettingsStore((s) => s.toggleNotificationChannel)

// Appearance
export const useAppearance = () =>
  useSettingsStore(
    useShallow((s) => s.appearance)
  )
export const useAppearanceActions = () =>
  useSettingsStore(
    useShallow((s) => ({
      setTheme: s.setTheme,
      setFontSize: s.setFontSize,
      setAccentColor: s.setAccentColor,
    }))
  )

// Save state
export const useIsSaving = () => useSettingsStore((s) => s.isSaving)
export const useSaveActions = () =>
  useSettingsStore(
    useShallow((s) => ({
      save: s.save,
      discard: s.discard,
    }))
  )

// Toast
export const useToast = () => useSettingsStore((s) => s.toast)
export const useDismissToast = () => useSettingsStore((s) => s.dismissToast)

// Dirty check: compare current state to saved snapshot
export const useIsDirty = () =>
  useSettingsStore((s) => {
    const profileDirty =
      s.profile.name !== s.savedProfile.name ||
      s.profile.email !== s.savedProfile.email ||
      s.profile.bio !== s.savedProfile.bio ||
      s.profile.avatarUrl !== s.savedProfile.avatarUrl

    const notificationsDirty = s.notifications.categories.some(
      (cat, i) => {
        const saved = s.savedNotifications.categories[i]
        if (!saved) return true
        return (
          cat.channels.email !== saved.channels.email ||
          cat.channels.push !== saved.channels.push ||
          cat.channels.sms !== saved.channels.sms
        )
      }
    )

    const appearanceDirty =
      s.appearance.theme !== s.savedAppearance.theme ||
      s.appearance.fontSize !== s.savedAppearance.fontSize ||
      s.appearance.accentColor !== s.savedAppearance.accentColor

    return profileDirty || notificationsDirty || appearanceDirty
  })
