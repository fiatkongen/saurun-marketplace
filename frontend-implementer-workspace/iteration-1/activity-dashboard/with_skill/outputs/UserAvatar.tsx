// ============================================
// UserAvatar Component
//
// Shows user image or initials fallback.
// ============================================

import { cn } from "./cn"
import type { UserAvatar as UserAvatarType } from "./types"

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2)
}

// Deterministic color from name hash
function getAvatarColor(name: string): string {
  const colors = [
    "bg-amber-600",
    "bg-sky-600",
    "bg-emerald-600",
    "bg-rose-600",
    "bg-violet-600",
    "bg-teal-600",
    "bg-orange-600",
    "bg-indigo-600",
  ]
  let hash = 0
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash)
  }
  return colors[Math.abs(hash) % colors.length]
}

interface UserAvatarProps {
  user: UserAvatarType
  size?: "sm" | "md"
}

export function UserAvatar({ user, size = "sm" }: UserAvatarProps) {
  const sizeClass = size === "sm" ? "h-7 w-7 text-[10px]" : "h-9 w-9 text-xs"

  if (user.imageUrl) {
    return (
      <img
        src={user.imageUrl}
        alt={user.name}
        className={cn(
          "shrink-0 rounded-full object-cover ring-1 ring-zinc-800",
          sizeClass
        )}
      />
    )
  }

  return (
    <div
      className={cn(
        "flex shrink-0 items-center justify-center rounded-full font-[DM_Sans] font-semibold text-white/90",
        getAvatarColor(user.name),
        sizeClass
      )}
      title={user.name}
    >
      {getInitials(user.name)}
    </div>
  )
}
