import { create } from "zustand";

// ---- Profile ----
export interface ProfileSettings {
  name: string;
  email: string;
  avatarUrl: string | null;
  bio: string;
}

// ---- Notifications ----
export interface NotificationChannel {
  email: boolean;
  push: boolean;
  sms: boolean;
}

export interface NotificationSettings {
  marketing: NotificationChannel;
  product: NotificationChannel;
  security: NotificationChannel;
  social: NotificationChannel;
}

// ---- Appearance ----
export type ThemeMode = "light" | "dark" | "system";

export interface AppearanceSettings {
  theme: ThemeMode;
  fontSize: number; // 12-24
  accentColor: string; // hex
}

// ---- Combined ----
export interface SettingsState {
  // Saved (server) values
  saved: {
    profile: ProfileSettings;
    notifications: NotificationSettings;
    appearance: AppearanceSettings;
  };

  // Current (draft) values
  draft: {
    profile: ProfileSettings;
    notifications: NotificationSettings;
    appearance: AppearanceSettings;
  };

  // Computed
  isDirty: boolean;
  isSaving: boolean;

  // Actions
  updateProfile: (patch: Partial<ProfileSettings>) => void;
  updateNotificationChannel: (
    category: keyof NotificationSettings,
    channel: keyof NotificationChannel,
    value: boolean,
  ) => void;
  updateAppearance: (patch: Partial<AppearanceSettings>) => void;
  save: () => Promise<void>;
  discard: () => void;
}

const defaultProfile: ProfileSettings = {
  name: "",
  email: "",
  avatarUrl: null,
  bio: "",
};

const defaultChannel: NotificationChannel = {
  email: true,
  push: true,
  sms: false,
};

const defaultNotifications: NotificationSettings = {
  marketing: { ...defaultChannel, email: true, push: false, sms: false },
  product: { ...defaultChannel },
  security: { email: true, push: true, sms: true },
  social: { ...defaultChannel, sms: false },
};

const defaultAppearance: AppearanceSettings = {
  theme: "system",
  fontSize: 16,
  accentColor: "#6366f1", // indigo-500
};

function deepEqual(a: unknown, b: unknown): boolean {
  return JSON.stringify(a) === JSON.stringify(b);
}

function computeDirty(state: Pick<SettingsState, "saved" | "draft">): boolean {
  return !deepEqual(state.saved, state.draft);
}

export const useSettingsStore = create<SettingsState>((set, get) => ({
  saved: {
    profile: { ...defaultProfile },
    notifications: structuredClone(defaultNotifications),
    appearance: { ...defaultAppearance },
  },
  draft: {
    profile: { ...defaultProfile },
    notifications: structuredClone(defaultNotifications),
    appearance: { ...defaultAppearance },
  },
  isDirty: false,
  isSaving: false,

  updateProfile: (patch) => {
    set((s) => {
      const draft = {
        ...s.draft,
        profile: { ...s.draft.profile, ...patch },
      };
      return { draft, isDirty: computeDirty({ saved: s.saved, draft }) };
    });
  },

  updateNotificationChannel: (category, channel, value) => {
    set((s) => {
      const draft = {
        ...s.draft,
        notifications: {
          ...s.draft.notifications,
          [category]: {
            ...s.draft.notifications[category],
            [channel]: value,
          },
        },
      };
      return { draft, isDirty: computeDirty({ saved: s.saved, draft }) };
    });
  },

  updateAppearance: (patch) => {
    set((s) => {
      const draft = {
        ...s.draft,
        appearance: { ...s.draft.appearance, ...patch },
      };
      return { draft, isDirty: computeDirty({ saved: s.saved, draft }) };
    });
  },

  save: async () => {
    set({ isSaving: true });
    // Simulate API call
    await new Promise((resolve) => setTimeout(resolve, 800));
    const { draft } = get();
    set({
      saved: structuredClone(draft),
      isDirty: false,
      isSaving: false,
    });
  },

  discard: () => {
    const { saved } = get();
    set({
      draft: structuredClone(saved),
      isDirty: false,
    });
  },
}));
