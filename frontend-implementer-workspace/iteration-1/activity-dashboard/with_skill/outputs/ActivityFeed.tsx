// ============================================
// ActivityFeed Component
//
// Infinite scroll list of activity items.
// Uses IntersectionObserver for "load more" trigger.
// ============================================

import { useRef, useCallback, useEffect } from "react"
import { cn } from "./cn"
import { useActivityFeed } from "./useDashboardQueries"
import {
  ActivityFeedItem,
  ActivityFeedItemSkeleton,
} from "./ActivityFeedItem"

export function ActivityFeed() {
  const {
    data,
    isPending,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useActivityFeed()

  // Infinite scroll sentinel
  const sentinelRef = useRef<HTMLDivElement>(null)

  const handleIntersection = useCallback(
    (entries: IntersectionObserverEntry[]) => {
      const [entry] = entries
      if (entry.isIntersecting && hasNextPage && !isFetchingNextPage) {
        fetchNextPage()
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage]
  )

  useEffect(() => {
    const sentinel = sentinelRef.current
    if (!sentinel) return

    const observer = new IntersectionObserver(handleIntersection, {
      rootMargin: "200px",
    })
    observer.observe(sentinel)

    return () => observer.disconnect()
  }, [handleIntersection])

  // Flatten pages
  const items = data?.pages.flatMap((page) => page.items) ?? []

  if (error) {
    return (
      <div
        data-testid="activity-feed-error"
        className="rounded-lg border border-red-500/20 bg-red-500/5 px-4 py-3 font-[DM_Sans] text-sm text-red-400"
      >
        Failed to load activity feed. {error.message}
      </div>
    )
  }

  return (
    <div data-testid="activity-feed" className="space-y-0">
      <h2 className="mb-4 font-[JetBrains_Mono] text-xs font-medium uppercase tracking-widest text-zinc-500">
        Recent Activity
      </h2>

      {isPending ? (
        <div data-testid="activity-feed-loading" className="space-y-0">
          {Array.from({ length: 6 }).map((_, i) => (
            <ActivityFeedItemSkeleton key={i} />
          ))}
        </div>
      ) : items.length === 0 ? (
        <div
          data-testid="activity-feed-empty"
          className="py-12 text-center font-[DM_Sans] text-sm text-zinc-600"
        >
          No activity found for the selected filters.
        </div>
      ) : (
        <>
          <div className="divide-y divide-zinc-800/50">
            {items.map((item, index) => (
              <ActivityFeedItem
                key={item.id}
                item={item}
                isLast={index === items.length - 1}
              />
            ))}
          </div>

          {/* Infinite scroll sentinel */}
          <div ref={sentinelRef} className="h-1" aria-hidden="true" />

          {isFetchingNextPage && (
            <div
              data-testid="activity-feed-loading-more"
              className="space-y-0 pt-2"
            >
              {Array.from({ length: 3 }).map((_, i) => (
                <ActivityFeedItemSkeleton key={i} />
              ))}
            </div>
          )}

          {!hasNextPage && items.length > 0 && (
            <p className="pb-2 pt-4 text-center font-[JetBrains_Mono] text-[11px] text-zinc-700">
              -- end of feed --
            </p>
          )}
        </>
      )}
    </div>
  )
}
