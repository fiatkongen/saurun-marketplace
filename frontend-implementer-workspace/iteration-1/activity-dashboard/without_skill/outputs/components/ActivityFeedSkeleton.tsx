// ============================================================
// ActivityFeedSkeleton.tsx — Loading skeleton for activity feed items
// ============================================================

import { Skeleton } from "../ui/skeleton";

interface ActivityFeedSkeletonProps {
  count?: number;
}

export function ActivityFeedSkeleton({ count = 5 }: ActivityFeedSkeletonProps) {
  return (
    <div className="space-y-3">
      {Array.from({ length: count }).map((_, i) => (
        <div key={i} className="flex items-start gap-4 rounded-lg border bg-card p-4">
          {/* Avatar skeleton */}
          <Skeleton className="size-10 shrink-0 rounded-full" />

          {/* Content skeleton */}
          <div className="flex-1 space-y-2">
            <div className="flex items-center gap-2">
              <Skeleton className="h-4 w-28" />
              <Skeleton className="h-5 w-16 rounded-full" />
            </div>
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-3 w-20" />
          </div>
        </div>
      ))}
    </div>
  );
}
