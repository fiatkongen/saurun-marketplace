// ============================================================
// FilterBar.tsx — Date range picker + activity type multi-select
// ============================================================

import { CalendarIcon, Filter, X } from "lucide-react";
import { format } from "date-fns";
import { useDashboardStore, ALL_ACTIVITY_TYPES } from "../store";
import type { ActivityType } from "../types";
import { Button } from "../ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import { Calendar } from "../ui/calendar";
import { Checkbox } from "../ui/checkbox";
import { Label } from "../ui/label";
import { Separator } from "../ui/separator";
import { Badge } from "../ui/badge";
import { cn } from "../lib/utils";

// ---------------------------------------------------------------------------
// Activity type labels
// ---------------------------------------------------------------------------

const TYPE_LABELS: Record<ActivityType, string> = {
  login: "Login",
  purchase: "Purchase",
  signup: "Signup",
};

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export function FilterBar() {
  const {
    dateRange,
    selectedActivityTypes,
    setDateRange,
    toggleActivityType,
    resetFilters,
  } = useDashboardStore();

  const hasActiveFilters =
    dateRange.from !== undefined ||
    dateRange.to !== undefined ||
    selectedActivityTypes.length > 0;

  return (
    <div className="flex flex-wrap items-center gap-3">
      <Filter className="size-4 text-muted-foreground" />

      {/* Date Range Picker */}
      <Popover>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            className={cn(
              "h-9 justify-start text-left font-normal",
              !dateRange.from && "text-muted-foreground"
            )}
          >
            <CalendarIcon className="mr-2 size-4" />
            {dateRange.from ? (
              dateRange.to ? (
                <>
                  {format(dateRange.from, "LLL dd, y")} - {format(dateRange.to, "LLL dd, y")}
                </>
              ) : (
                format(dateRange.from, "LLL dd, y")
              )
            ) : (
              "Pick a date range"
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            initialFocus
            mode="range"
            defaultMonth={dateRange.from}
            selected={{ from: dateRange.from, to: dateRange.to }}
            onSelect={(range) =>
              setDateRange({ from: range?.from, to: range?.to })
            }
            numberOfMonths={2}
          />
        </PopoverContent>
      </Popover>

      <Separator orientation="vertical" className="h-6" />

      {/* Activity Type Multi-Select */}
      <Popover>
        <PopoverTrigger asChild>
          <Button variant="outline" className="h-9">
            Activity Type
            {selectedActivityTypes.length > 0 && (
              <Badge variant="secondary" className="ml-2 rounded-full px-1.5 text-xs">
                {selectedActivityTypes.length}
              </Badge>
            )}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-48 p-3" align="start">
          <div className="space-y-3">
            <p className="text-xs font-medium text-muted-foreground">Filter by type</p>
            {ALL_ACTIVITY_TYPES.map((type) => (
              <div key={type} className="flex items-center gap-2">
                <Checkbox
                  id={`filter-${type}`}
                  checked={selectedActivityTypes.includes(type)}
                  onCheckedChange={() => toggleActivityType(type)}
                />
                <Label
                  htmlFor={`filter-${type}`}
                  className="cursor-pointer text-sm font-normal"
                >
                  {TYPE_LABELS[type]}
                </Label>
              </div>
            ))}
          </div>
        </PopoverContent>
      </Popover>

      {/* Active filter badges */}
      {selectedActivityTypes.map((type) => (
        <Badge
          key={type}
          variant="secondary"
          className="cursor-pointer gap-1 pr-1"
          onClick={() => toggleActivityType(type)}
        >
          {TYPE_LABELS[type]}
          <X className="size-3" />
        </Badge>
      ))}

      {/* Reset filters */}
      {hasActiveFilters && (
        <Button
          variant="ghost"
          size="sm"
          className="h-7 text-xs text-muted-foreground"
          onClick={resetFilters}
        >
          Reset filters
        </Button>
      )}
    </div>
  );
}
