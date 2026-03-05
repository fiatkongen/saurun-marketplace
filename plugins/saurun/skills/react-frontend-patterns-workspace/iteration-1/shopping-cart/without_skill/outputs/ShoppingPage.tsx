import React from "react";
import { ProductGrid } from "./ProductGrid";
import { CartSidebar } from "./CartSidebar";
import { CartButton } from "./CartButton";

export function ShoppingPage() {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="sticky top-0 z-30 border-b bg-white">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <h1 className="text-xl font-bold text-gray-900">Shop</h1>
          <CartButton />
        </div>
      </header>

      {/* Main content */}
      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <ProductGrid />
      </main>

      {/* Cart sidebar */}
      <CartSidebar />
    </div>
  );
}
