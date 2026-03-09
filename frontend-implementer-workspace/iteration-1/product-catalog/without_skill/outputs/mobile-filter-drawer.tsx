// ============================================================================
// MobileFilterDrawer — Responsive filter panel for small screens
// ============================================================================

import { SlidersHorizontal } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { FilterSidebar } from "./filter-sidebar";
import type { FilterState } from "./types";

interface MobileFilterDrawerProps {
  filters: FilterState;
  onFilterChange: (partial: Partial<FilterState>) => void;
  activeFilterCount: number;
}

export function MobileFilterDrawer({
  filters,
  onFilterChange,
  activeFilterCount,
}: MobileFilterDrawerProps) {
  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="outline" size="sm" className="lg:hidden gap-2">
          <SlidersHorizontal className="h-4 w-4" />
          Filters
          {activeFilterCount > 0 && (
            <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-bold text-primary-foreground">
              {activeFilterCount}
            </span>
          )}
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-80 overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Filters</SheetTitle>
        </SheetHeader>
        <div className="mt-4">
          <FilterSidebar filters={filters} onFilterChange={onFilterChange} />
        </div>
      </SheetContent>
    </Sheet>
  );
}
