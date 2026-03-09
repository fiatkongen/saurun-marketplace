// ============================================================================
// CartBadge — Header cart icon with item count
// ============================================================================

import { ShoppingCart } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCartStore } from "./cart-store";

export function CartBadge() {
  const totalItems = useCartStore((s) => s.totalItems());

  return (
    <Button variant="ghost" size="icon" className="relative">
      <ShoppingCart className="h-5 w-5" />
      {totalItems > 0 && (
        <span className="absolute -top-1 -right-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-bold text-primary-foreground">
          {totalItems > 99 ? "99+" : totalItems}
        </span>
      )}
      <span className="sr-only">Cart ({totalItems} items)</span>
    </Button>
  );
}
