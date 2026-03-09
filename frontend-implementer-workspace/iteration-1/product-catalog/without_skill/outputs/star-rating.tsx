// ============================================================================
// StarRating — Display or interactive star rating
// ============================================================================

import { Star } from "lucide-react";
import { cn } from "@/lib/utils";

interface StarRatingProps {
  rating: number;
  maxRating?: number;
  interactive?: boolean;
  onRate?: (rating: number) => void;
  size?: "sm" | "md";
}

export function StarRating({
  rating,
  maxRating = 5,
  interactive = false,
  onRate,
  size = "md",
}: StarRatingProps) {
  const sizeClass = size === "sm" ? "h-3.5 w-3.5" : "h-5 w-5";

  return (
    <div className="flex items-center gap-0.5">
      {Array.from({ length: maxRating }, (_, i) => {
        const starValue = i + 1;
        const filled = starValue <= rating;
        const halfFilled = !filled && starValue - 0.5 <= rating;

        return (
          <button
            key={i}
            type="button"
            disabled={!interactive}
            onClick={() => interactive && onRate?.(starValue)}
            className={cn(
              "relative p-0 border-0 bg-transparent",
              interactive && "cursor-pointer hover:scale-110 transition-transform",
              !interactive && "cursor-default"
            )}
          >
            <Star
              className={cn(
                sizeClass,
                filled
                  ? "fill-yellow-400 text-yellow-400"
                  : halfFilled
                    ? "fill-yellow-400/50 text-yellow-400"
                    : "fill-transparent text-muted-foreground/40"
              )}
            />
          </button>
        );
      })}
    </div>
  );
}
