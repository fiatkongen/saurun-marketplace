/**
 * StatCard
 *
 * Presentational component for a single statistic.
 * Pure props in, no state management. Parent passes data.
 */

interface StatCardProps {
  label: string
  value: string | number
  description?: string
  trend?: {
    value: number
    isPositive: boolean
  }
}

export function StatCard({ label, value, description, trend }: StatCardProps) {
  return (
    <div
      className="rounded-lg border bg-card p-6 shadow-sm"
      data-testid={`stat-card-${label.toLowerCase().replace(/\s+/g, '-')}`}
    >
      <p className="text-sm font-medium text-muted-foreground">{label}</p>
      <div className="mt-2 flex items-baseline gap-2">
        <p className="text-3xl font-bold tracking-tight">{value}</p>
        {trend && (
          <span
            className={`text-sm font-medium ${
              trend.isPositive ? 'text-green-600' : 'text-red-600'
            }`}
            data-testid={`stat-card-trend-${label.toLowerCase().replace(/\s+/g, '-')}`}
          >
            {trend.isPositive ? '+' : ''}
            {trend.value}%
          </span>
        )}
      </div>
      {description && (
        <p className="mt-1 text-xs text-muted-foreground">{description}</p>
      )}
    </div>
  )
}
