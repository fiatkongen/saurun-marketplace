// ============================================
// Relative Time Formatter
// ============================================

const MINUTE = 60
const HOUR = 3600
const DAY = 86400
const WEEK = 604800

export function formatRelativeTime(isoTimestamp: string): string {
  const now = Date.now()
  const then = new Date(isoTimestamp).getTime()
  const diffSeconds = Math.floor((now - then) / 1000)

  if (diffSeconds < 0) return "just now"
  if (diffSeconds < 30) return "just now"
  if (diffSeconds < MINUTE) return `${diffSeconds}s ago`
  if (diffSeconds < HOUR) {
    const mins = Math.floor(diffSeconds / MINUTE)
    return `${mins}m ago`
  }
  if (diffSeconds < DAY) {
    const hrs = Math.floor(diffSeconds / HOUR)
    return `${hrs}h ago`
  }
  if (diffSeconds < WEEK) {
    const days = Math.floor(diffSeconds / DAY)
    return `${days}d ago`
  }

  return new Date(isoTimestamp).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  })
}
