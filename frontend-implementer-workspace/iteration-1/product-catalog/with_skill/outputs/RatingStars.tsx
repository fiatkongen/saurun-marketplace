// =============================================================================
// RatingStars — Renders filled/half/empty stars for a rating value
// =============================================================================

import { cn } from "./cn"

interface RatingStarsProps {
  rating: number
  maxStars?: number
  size?: "sm" | "md" | "lg"
  interactive?: boolean
  onRate?: (rating: number) => void
  className?: string
}

const sizeMap = {
  sm: "w-3.5 h-3.5",
  md: "w-4.5 h-4.5",
  lg: "w-5.5 h-5.5",
}

function StarIcon({
  fill,
  size,
  className,
}: {
  fill: "full" | "half" | "empty"
  size: "sm" | "md" | "lg"
  className?: string
}) {
  const dims = sizeMap[size]

  if (fill === "full") {
    return (
      <svg
        className={cn(dims, "text-amber-500", className)}
        viewBox="0 0 20 20"
        fill="currentColor"
        aria-hidden="true"
      >
        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
      </svg>
    )
  }

  if (fill === "half") {
    return (
      <svg
        className={cn(dims, className)}
        viewBox="0 0 20 20"
        aria-hidden="true"
      >
        <defs>
          <linearGradient id="halfGrad">
            <stop offset="50%" className="[stop-color:theme(colors.amber.500)]" />
            <stop offset="50%" className="[stop-color:theme(colors.stone.300)]" />
          </linearGradient>
        </defs>
        <path
          fill="url(#halfGrad)"
          d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
        />
      </svg>
    )
  }

  return (
    <svg
      className={cn(dims, "text-stone-300", className)}
      viewBox="0 0 20 20"
      fill="currentColor"
      aria-hidden="true"
    >
      <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
    </svg>
  )
}

export function RatingStars({
  rating,
  maxStars = 5,
  size = "md",
  interactive = false,
  onRate,
  className,
}: RatingStarsProps) {
  const stars = Array.from({ length: maxStars }, (_, i) => {
    const starValue = i + 1
    if (rating >= starValue) return "full" as const
    if (rating >= starValue - 0.5) return "half" as const
    return "empty" as const
  })

  return (
    <div
      className={cn("flex items-center gap-0.5", className)}
      role={interactive ? "radiogroup" : "img"}
      aria-label={interactive ? "Rate this product" : `${rating} out of ${maxStars} stars`}
    >
      {stars.map((fill, i) => {
        if (interactive) {
          return (
            <button
              key={i}
              type="button"
              data-testid={`rating-star-${i + 1}`}
              className="cursor-pointer transition-transform hover:scale-110"
              onClick={() => onRate?.(i + 1)}
              aria-label={`${i + 1} star${i === 0 ? "" : "s"}`}
            >
              <StarIcon
                fill={fill}
                size={size}
              />
            </button>
          )
        }
        return <StarIcon key={i} fill={fill} size={size} />
      })}
    </div>
  )
}
