// ============================================================
// ActivityFeedItem.tsx — Single activity feed entry
// ============================================================

import { LogIn, ShoppingCart, UserPlus } from "lucide-react";
import type { ActivityItem, ActivityType } from "../types";
import { formatRelativeTime } from "../hooks";
import { Avatar, AvatarFallback, AvatarImage } from "../ui/avatar";
import { Badge } from "../ui/badge";
import { cn } from "../lib/utils";

// ---------------------------------------------------------------------------
// Activity type config
// ---------------------------------------------------------------------------

const ACTIVITY_CONFIG: Record<
  ActivityType,
  {
    icon: React.ComponentType<{ className?: string }>;
    label: string;
    badgeVariant: "default" | "secondary" | "outline";
    colorClass: string;
  }
> = {
  login: {
    icon: LogIn,
    label: "Login",
    badgeVariant: "secondary",
    colorClass: "text-blue-600 dark:text-blue-400",
  },
  purchase: {
    icon: ShoppingCart,
    label: "Purchase",
    badgeVariant: "default",
    colorClass: "text-emerald-600 dark:text-emerald-400",
  },
  signup: {
    icon: UserPlus,
    label: "Signup",
    badgeVariant: "outline",
    colorClass: "text-violet-600 dark:text-violet-400",
  },
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

interface ActivityFeedItemProps {
  activity: ActivityItem;
}

export function ActivityFeedItem({ activity }: ActivityFeedItemProps) {
  const config = ACTIVITY_CONFIG[activity.type];
  const Icon = config.icon;
  const initials = activity.user.name
    .split(" ")
    .map((n) => n[0])
    .join("")
    .toUpperCase();

  return (
    <div className="flex items-start gap-4 rounded-lg border bg-card p-4 transition-colors hover:bg-accent/50">
      {/* Avatar */}
      <Avatar className="size-10 shrink-0">
        <AvatarImage src={activity.user.avatarUrl} alt={activity.user.name} />
        <AvatarFallback className="text-xs font-medium">{initials}</AvatarFallback>
      </Avatar>

      {/* Content */}
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <span className="truncate font-medium text-sm">{activity.user.name}</span>
          <Badge variant={config.badgeVariant} className="shrink-0 text-xs">
            <Icon className={cn("mr-1 size-3", config.colorClass)} />
            {config.label}
          </Badge>
        </div>
        <p className="mt-0.5 text-sm text-muted-foreground">{activity.description}</p>
        <time
          className="mt-1 block text-xs text-muted-foreground/70"
          dateTime={activity.timestamp}
          title={new Date(activity.timestamp).toLocaleString()}
        >
          {formatRelativeTime(activity.timestamp)}
        </time>
      </div>
    </div>
  );
}
