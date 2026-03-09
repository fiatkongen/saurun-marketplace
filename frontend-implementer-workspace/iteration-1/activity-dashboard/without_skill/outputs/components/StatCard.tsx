// ============================================================
// StatCard.tsx — Single stat card with trend indicator + hover animation
// ============================================================

import { ArrowDownRight, ArrowUpRight, Minus, Users, DollarSign, Activity, Target } from "lucide-react";
import type { StatCardData, TrendDirection } from "../types";
import { Card, CardContent } from "../ui/card";
import { cn } from "../lib/utils";

// ---------------------------------------------------------------------------
// Icon map
// ---------------------------------------------------------------------------

const ICON_MAP: Record<string, React.ComponentType<{ className?: string }>> = {
  users: Users,
  "dollar-sign": DollarSign,
  activity: Activity,
  target: Target,
};

// ---------------------------------------------------------------------------
// Trend config
// ---------------------------------------------------------------------------

const TREND_CONFIG: Record<
  TrendDirection,
  { icon: React.ComponentType<{ className?: string }>; colorClass: string; bgClass: string }
> = {
  up: {
    icon: ArrowUpRight,
    colorClass: "text-emerald-600 dark:text-emerald-400",
    bgClass: "bg-emerald-50 dark:bg-emerald-950/50",
  },
  down: {
    icon: ArrowDownRight,
    colorClass: "text-red-600 dark:text-red-400",
    bgClass: "bg-red-50 dark:bg-red-950/50",
  },
  neutral: {
    icon: Minus,
    colorClass: "text-muted-foreground",
    bgClass: "bg-muted",
  },
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

interface StatCardProps {
  data: StatCardData;
}

export function StatCard({ data }: StatCardProps) {
  const { label, value, trend, icon } = data;
  const trendConfig = TREND_CONFIG[trend.direction];
  const TrendIcon = trendConfig.icon;
  const CardIcon = ICON_MAP[icon] ?? Activity;

  return (
    <Card
      className={cn(
        "group relative overflow-hidden transition-all duration-300 ease-out",
        "hover:shadow-lg hover:-translate-y-1 hover:shadow-black/5",
        "dark:hover:shadow-black/20"
      )}
    >
      <CardContent className="p-6">
        {/* Background icon decoration */}
        <div className="absolute -right-3 -top-3 opacity-[0.04] transition-opacity duration-300 group-hover:opacity-[0.08]">
          <CardIcon className="size-24" />
        </div>

        <div className="flex items-start justify-between">
          {/* Icon */}
          <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <CardIcon className="size-5" />
          </div>

          {/* Trend badge */}
          <div
            className={cn(
              "flex items-center gap-0.5 rounded-full px-2 py-1 text-xs font-medium",
              trendConfig.bgClass,
              trendConfig.colorClass
            )}
          >
            <TrendIcon className="size-3.5" />
            <span>{trend.percentage}%</span>
          </div>
        </div>

        {/* Value */}
        <div className="mt-4">
          <p className="text-2xl font-bold tracking-tight">{value}</p>
          <p className="mt-1 text-sm text-muted-foreground">{label}</p>
        </div>
      </CardContent>
    </Card>
  );
}
