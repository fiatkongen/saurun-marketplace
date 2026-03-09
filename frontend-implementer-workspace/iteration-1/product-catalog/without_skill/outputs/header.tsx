// ============================================================================
// Header — Top bar with logo and cart badge
// ============================================================================

import { Store } from "lucide-react";
import { CartBadge } from "./cart-badge";

export function Header() {
  return (
    <header className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="mx-auto flex h-14 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
        <div className="flex items-center gap-2">
          <Store className="h-6 w-6 text-primary" />
          <span className="text-lg font-bold tracking-tight">StoreFront</span>
        </div>
        <CartBadge />
      </div>
    </header>
  );
}
