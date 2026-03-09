// ============================================
// ActivityFeedItem Component
//
// Single row in the activity feed with timeline spine.
// ============================================

import { cn } from "./cn"
import { ActivityIcon } from "./ActivityIcon"
import { UserAvatar } from "./UserAvatar"
import { formatRelativeTime } from "./formatRelativeTime"
import type { ActivityItem } from "./types"

interface ActivityFeedItemProps {
  item: ActivityItem
  isLast: boolean
}

export function ActivityFeedItem({ item, isLast }: ActivityFeedItemProps) {
  return (
    <div
      data-testid={`activity-item-${item.id}`}
      className="group relative flex gap-3 py-3"
    >
      {/* Timeline spine */}
      {!isLast && (
        <div
          className="absolute left-[15px] top-11 bottom-0 w-px bg-zinc-800"
          aria-hidden="true"
        />
      )}

      {/* Type icon */}
      <ActivityIcon type={item.type} />

      {/* Content */}
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <UserAvatar user={item.user} />
          <span className="truncate font-[DM_Sans] text-sm font-medium text-zinc-200">
            {item.user.name}
          </span>
          <span
            className={cn(
              "inline-flex rounded-full px-2 py-0.5",
              "font-[JetBrains_Mono] text-[10px] font-medium uppercase tracking-wider",
              item.type === "login" && "bg-sky-500/10 text-sky-400",
              item.type === "purchase" && "bg-emerald-500/10 text-emerald-400",
              item.type === "signup" && "bg-amber-500/10 text-amber-400"
            )}
          >
            {item.type}
          </span>
        </div>

        <p className="mt-1 font-[DM_Sans] text-sm text-zinc-400">
          {item.description}
        </p>

        <time
          dateTime={item.timestamp}
          className="mt-0.5 block font-[JetBrains_Mono] text-[11px] text-zinc-600"
        >
          {formatRelativeTime(item.timestamp)}
        </time>
      </div>
    </div>
  )
}

// ============================================
// Skeleton
// ============================================

export function ActivityFeedItemSkeleton() {
  return (
    <div className="flex gap-3 py-3">
      <div className="h-8 w-8 shrink-0 animate-pulse rounded-full bg-zinc-800" />
      <div className="flex-1 space-y-2">
        <div className="flex items-center gap-2">
          <div className="h-7 w-7 animate-pulse rounded-full bg-zinc-800" />
          <div className="h-3.5 w-24 animate-pulse rounded-sm bg-zinc-800" />
          <div className="h-4 w-14 animate-pulse rounded-full bg-zinc-800" />
        </div>
        <div className="h-3.5 w-64 animate-pulse rounded-sm bg-zinc-800" />
        <div className="h-3 w-16 animate-pulse rounded-sm bg-zinc-800/60" />
      </div>
    </div>
  )
}
