// ============================================================================
// FilterSidebar — Collapsible filter groups
// ============================================================================

import { useState } from "react";
import { ChevronDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Slider } from "@/components/ui/slider";
import { Switch } from "@/components/ui/switch";
import { cn } from "@/lib/utils";
import type { FilterState } from "./types";
import { CATEGORIES, PRICE_MIN, PRICE_MAX } from "./data";
import { StarRating } from "./star-rating";

interface FilterSidebarProps {
  filters: FilterState;
  onFilterChange: (partial: Partial<FilterState>) => void;
}

// ---------------------------------------------------------------------------
// Collapsible group wrapper
// ---------------------------------------------------------------------------

function FilterGroup({
  title,
  defaultOpen = true,
  children,
}: {
  title: string;
  defaultOpen?: boolean;
  children: React.ReactNode;
}) {
  const [open, setOpen] = useState(defaultOpen);

  return (
    <div className="border-b border-border pb-4">
      <button
        type="button"
        onClick={() => setOpen(!open)}
        className="flex w-full items-center justify-between py-2 text-sm font-semibold text-foreground hover:text-foreground/80 transition-colors"
      >
        {title}
        <ChevronDown
          className={cn(
            "h-4 w-4 transition-transform duration-200",
            open && "rotate-180"
          )}
        />
      </button>
      <div
        className={cn(
          "grid transition-all duration-200",
          open ? "grid-rows-[1fr] opacity-100 mt-2" : "grid-rows-[0fr] opacity-0"
        )}
      >
        <div className="overflow-hidden">{children}</div>
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Main sidebar
// ---------------------------------------------------------------------------

export function FilterSidebar({ filters, onFilterChange }: FilterSidebarProps) {
  const hasActiveFilters =
    filters.categories.length > 0 ||
    filters.priceRange[0] !== PRICE_MIN ||
    filters.priceRange[1] !== PRICE_MAX ||
    filters.minRating > 0 ||
    filters.inStockOnly;

  return (
    <aside className="w-64 shrink-0 space-y-1">
      <div className="flex items-center justify-between mb-2">
        <h2 className="text-lg font-semibold">Filters</h2>
        {hasActiveFilters && (
          <Button
            variant="ghost"
            size="sm"
            className="h-7 text-xs"
            onClick={() =>
              onFilterChange({
                categories: [],
                priceRange: [PRICE_MIN, PRICE_MAX],
                minRating: 0,
                inStockOnly: false,
              })
            }
          >
            Clear all
          </Button>
        )}
      </div>

      {/* Price Range */}
      <FilterGroup title="Price Range">
        <div className="space-y-3 px-1">
          <Slider
            min={PRICE_MIN}
            max={PRICE_MAX}
            step={10}
            value={filters.priceRange}
            onValueChange={(value) =>
              onFilterChange({ priceRange: value as [number, number] })
            }
          />
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>${filters.priceRange[0]}</span>
            <span>${filters.priceRange[1]}</span>
          </div>
        </div>
      </FilterGroup>

      {/* Categories */}
      <FilterGroup title="Category">
        <div className="space-y-2">
          {CATEGORIES.map((cat) => {
            const checked = filters.categories.includes(cat);
            return (
              <div key={cat} className="flex items-center gap-2">
                <Checkbox
                  id={`cat-${cat}`}
                  checked={checked}
                  onCheckedChange={(val) => {
                    const next = val
                      ? [...filters.categories, cat]
                      : filters.categories.filter((c) => c !== cat);
                    onFilterChange({ categories: next });
                  }}
                />
                <Label htmlFor={`cat-${cat}`} className="text-sm cursor-pointer">
                  {cat}
                </Label>
              </div>
            );
          })}
        </div>
      </FilterGroup>

      {/* Rating */}
      <FilterGroup title="Minimum Rating">
        <div className="space-y-2 px-1">
          {[4, 3, 2, 1].map((r) => (
            <button
              key={r}
              type="button"
              onClick={() =>
                onFilterChange({ minRating: filters.minRating === r ? 0 : r })
              }
              className={cn(
                "flex items-center gap-2 w-full rounded-md px-2 py-1 text-sm transition-colors",
                filters.minRating === r
                  ? "bg-accent text-accent-foreground"
                  : "hover:bg-muted"
              )}
            >
              <StarRating rating={r} size="sm" />
              <span className="text-muted-foreground">& up</span>
            </button>
          ))}
        </div>
      </FilterGroup>

      {/* In Stock */}
      <FilterGroup title="Availability">
        <div className="flex items-center justify-between px-1">
          <Label htmlFor="in-stock-toggle" className="text-sm cursor-pointer">
            In stock only
          </Label>
          <Switch
            id="in-stock-toggle"
            checked={filters.inStockOnly}
            onCheckedChange={(val) => onFilterChange({ inStockOnly: val })}
          />
        </div>
      </FilterGroup>
    </aside>
  );
}
