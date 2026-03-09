import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  useSettingsStore,
  type NotificationChannel,
  type NotificationSettings,
} from "./settings-store";

interface CategoryConfig {
  key: keyof NotificationSettings;
  label: string;
  description: string;
}

const categories: CategoryConfig[] = [
  {
    key: "product",
    label: "Product updates",
    description: "New features, improvements, and changelog.",
  },
  {
    key: "security",
    label: "Security alerts",
    description: "Login attempts, password changes, and suspicious activity.",
  },
  {
    key: "social",
    label: "Social",
    description: "Comments, mentions, and follows.",
  },
  {
    key: "marketing",
    label: "Marketing",
    description: "Promotions, newsletters, and special offers.",
  },
];

const channels: { key: keyof NotificationChannel; label: string }[] = [
  { key: "email", label: "Email" },
  { key: "push", label: "Push" },
  { key: "sms", label: "SMS" },
];

export function NotificationsTab() {
  const notifications = useSettingsStore((s) => s.draft.notifications);
  const updateChannel = useSettingsStore((s) => s.updateNotificationChannel);

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-lg font-semibold text-foreground">Notifications</h2>
        <p className="text-sm text-muted-foreground mt-1">
          Choose how and when you want to be notified.
        </p>
      </div>

      {/* Channel header row — visible on md+ */}
      <div className="hidden md:grid md:grid-cols-[1fr_80px_80px_80px] gap-4 items-center border-b border-border pb-2 px-1">
        <span className="text-xs font-medium text-muted-foreground uppercase tracking-wider">
          Category
        </span>
        {channels.map((ch) => (
          <span
            key={ch.key}
            className="text-xs font-medium text-muted-foreground uppercase tracking-wider text-center"
          >
            {ch.label}
          </span>
        ))}
      </div>

      {/* Category rows */}
      <div className="space-y-6 md:space-y-0 md:divide-y md:divide-border">
        {categories.map((cat) => (
          <div
            key={cat.key}
            className="rounded-lg border border-border p-4 md:border-0 md:rounded-none md:px-1 md:py-4 md:grid md:grid-cols-[1fr_80px_80px_80px] md:gap-4 md:items-center"
          >
            {/* Label + description */}
            <div className="mb-4 md:mb-0">
              <p className="text-sm font-medium text-foreground">{cat.label}</p>
              <p className="text-xs text-muted-foreground mt-0.5">
                {cat.description}
              </p>
            </div>

            {/* Toggles */}
            <div className="flex items-center gap-6 md:contents">
              {channels.map((ch) => {
                const id = `${cat.key}-${ch.key}`;
                return (
                  <div
                    key={ch.key}
                    className="flex items-center gap-2 md:justify-center"
                  >
                    {/* Show label on mobile, hidden on md+ (header row covers it) */}
                    <Label
                      htmlFor={id}
                      className="text-xs text-muted-foreground md:sr-only"
                    >
                      {ch.label}
                    </Label>
                    <Switch
                      id={id}
                      checked={notifications[cat.key][ch.key]}
                      onCheckedChange={(checked) =>
                        updateChannel(cat.key, ch.key, checked)
                      }
                    />
                  </div>
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
