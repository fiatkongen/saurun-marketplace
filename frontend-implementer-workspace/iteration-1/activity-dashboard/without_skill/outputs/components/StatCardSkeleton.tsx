// ============================================================
// StatCardSkeleton.tsx — Loading skeleton for stat cards
// ============================================================

import { Card, CardContent } from "../ui/card";
import { Skeleton } from "../ui/skeleton";

export function StatCardSkeleton() {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-start justify-between">
          <Skeleton className="size-10 rounded-lg" />
          <Skeleton className="h-6 w-16 rounded-full" />
        </div>
        <div className="mt-4 space-y-2">
          <Skeleton className="h-8 w-28" />
          <Skeleton className="h-4 w-20" />
        </div>
      </CardContent>
    </Card>
  );
}
