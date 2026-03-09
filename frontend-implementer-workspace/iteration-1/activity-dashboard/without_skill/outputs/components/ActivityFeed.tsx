// ============================================================
// ActivityFeed.tsx — Infinite-scroll activity feed
// ============================================================

import { useCallback, useEffect, useRef } from "react";
import { Loader2 } from "lucide-react";
import { useFlatActivityItems } from "../hooks";
import { ActivityFeedItem } from "./ActivityFeedItem";
import { ActivityFeedSkeleton } from "./ActivityFeedSkeleton";

export function ActivityFeed() {
  const {
    items,
    isLoading,
    isError,
    isFetchingNextPage,
    hasNextPage,
    fetchNextPage,
  } = useFlatActivityItems();

  // -----------------------------------------------------------------------
  // Intersection Observer for infinite scroll
  // -----------------------------------------------------------------------

  const sentinelRef = useRef<HTMLDivElement>(null);

  const handleIntersection = useCallback(
    (entries: IntersectionObserverEntry[]) => {
      const [entry] = entries;
      if (entry.isIntersecting && hasNextPage && !isFetchingNextPage) {
        fetchNextPage();
      }
    },
    [hasNextPage, isFetchingNextPage, fetchNextPage]
  );

  useEffect(() => {
    const sentinel = sentinelRef.current;
    if (!sentinel) return;

    const observer = new IntersectionObserver(handleIntersection, {
      root: null,
      rootMargin: "200px",
      threshold: 0,
    });

    observer.observe(sentinel);
    return () => observer.disconnect();
  }, [handleIntersection]);

  // -----------------------------------------------------------------------
  // Render
  // -----------------------------------------------------------------------

  if (isError) {
    return (
      <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
        Failed to load activity feed. Please try again.
      </div>
    );
  }

  if (isLoading) {
    return <ActivityFeedSkeleton count={5} />;
  }

  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-dashed p-8 text-center text-sm text-muted-foreground">
        No activity found matching your filters.
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {items.map((activity) => (
        <ActivityFeedItem key={activity.id} activity={activity} />
      ))}

      {/* Infinite scroll sentinel */}
      <div ref={sentinelRef} className="h-1" />

      {/* Loading more indicator */}
      {isFetchingNextPage && (
        <div className="flex items-center justify-center py-4">
          <Loader2 className="size-5 animate-spin text-muted-foreground" />
          <span className="ml-2 text-sm text-muted-foreground">Loading more...</span>
        </div>
      )}

      {/* End of feed */}
      {!hasNextPage && items.length > 0 && (
        <p className="py-4 text-center text-xs text-muted-foreground">
          You've reached the end of the activity feed.
        </p>
      )}
    </div>
  );
}
