// ============================================
// Activity Type Icons
//
// Distinct icon per activity type with color coding.
// ============================================

import { cn } from "./cn"
import type { ActivityType } from "./types"

const iconConfig: Record<
  ActivityType,
  { bgClass: string; textClass: string; label: string }
> = {
  login: {
    bgClass: "bg-sky-500/10",
    textClass: "text-sky-400",
    label: "Login",
  },
  purchase: {
    bgClass: "bg-emerald-500/10",
    textClass: "text-emerald-400",
    label: "Purchase",
  },
  signup: {
    bgClass: "bg-amber-500/10",
    textClass: "text-amber-400",
    label: "Signup",
  },
}

function LoginIcon({ className }: { className?: string }) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 16 16"
      fill="none"
      className={className}
      aria-hidden="true"
    >
      <path
        d="M6 2h6a2 2 0 012 2v8a2 2 0 01-2 2H6M2 8h8m0 0L7 5m3 3l-3 3"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function PurchaseIcon({ className }: { className?: string }) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 16 16"
      fill="none"
      className={className}
      aria-hidden="true"
    >
      <path
        d="M1 1h2l1.5 8h8L14 4H4.5M6 13a1 1 0 11-2 0 1 1 0 012 0zm8 0a1 1 0 11-2 0 1 1 0 012 0z"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

function SignupIcon({ className }: { className?: string }) {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 16 16"
      fill="none"
      className={className}
      aria-hidden="true"
    >
      <path
        d="M11 5a3 3 0 11-6 0 3 3 0 016 0zM2 14a6 6 0 0112 0M13 3v4m-2-2h4"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

const iconComponents: Record<
  ActivityType,
  React.ComponentType<{ className?: string }>
> = {
  login: LoginIcon,
  purchase: PurchaseIcon,
  signup: SignupIcon,
}

interface ActivityIconProps {
  type: ActivityType
}

export function ActivityIcon({ type }: ActivityIconProps) {
  const config = iconConfig[type]
  const IconComp = iconComponents[type]

  return (
    <div
      className={cn(
        "flex h-8 w-8 shrink-0 items-center justify-center rounded-full",
        config.bgClass
      )}
      title={config.label}
    >
      <IconComp className={config.textClass} />
    </div>
  )
}
