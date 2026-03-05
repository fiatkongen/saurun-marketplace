/**
 * ShopPage
 *
 * Route-level component composing product grid + cart sidebar.
 * Error boundaries at page level (wraps everything) and feature level
 * (isolates cart sidebar so a cart error doesn't crash the product grid).
 */

import { ErrorBoundary } from 'react-error-boundary'
import { ProductGrid } from '../components/products/ProductGrid'
import { CartSidebar } from '../components/cart/CartSidebar'
import { CartButton } from '../components/cart/CartButton'

export function ShopPage() {
  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="sticky top-0 z-30 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container flex h-14 items-center justify-between">
          <h1 className="text-xl font-bold">Shop</h1>
          <CartButton />
        </div>
      </header>

      {/* Main content */}
      <main className="container py-8">
        <ProductGrid />
      </main>

      {/* Cart sidebar -- feature-level error boundary isolates from page */}
      <ErrorBoundary
        fallback={
          <div className="fixed bottom-4 right-4 rounded-md border border-destructive/50 bg-destructive/10 p-4 text-sm text-destructive">
            Cart unavailable. Please refresh.
          </div>
        }
      >
        <CartSidebar />
      </ErrorBoundary>
    </div>
  )
}
