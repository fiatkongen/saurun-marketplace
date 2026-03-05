/**
 * CartSidebar
 *
 * Slide-over sidebar showing cart items, quantity controls, and total.
 * Uses selector hooks from cart store -- never bare useCartStore().
 * Derived state (total, count) computed in selectors, not stored.
 */

import {
  useCartItems,
  useCartIsOpen,
  useCartTotal,
  useCartCount,
  useCartActions,
  useCartSidebarActions,
} from '../../stores/useCartStore'
import { CartItemRow } from './CartItemRow'

export function CartSidebar() {
  const items = useCartItems()
  const isOpen = useCartIsOpen()
  const total = useCartTotal()
  const count = useCartCount()
  const { updateQuantity, removeItem, clearCart } = useCartActions()
  const { closeSidebar } = useCartSidebarActions()

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          data-testid="cart-sidebar-backdrop"
          className="fixed inset-0 z-40 bg-black/50 transition-opacity"
          onClick={closeSidebar}
        />
      )}

      {/* Sidebar panel */}
      <div
        data-testid="cart-sidebar"
        className={`fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col bg-background shadow-xl transition-transform duration-300 ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
      >
        {/* Header */}
        <div className="flex items-center justify-between border-b px-6 py-4">
          <h2 className="text-lg font-semibold">
            Cart{count > 0 && ` (${count})`}
          </h2>
          <button
            data-testid="cart-sidebar-close"
            onClick={closeSidebar}
            className="text-muted-foreground transition-colors hover:text-foreground"
            aria-label="Close cart"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="20"
              height="20"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>

        {/* Items */}
        <div className="flex-1 overflow-y-auto px-6">
          {items.length === 0 ? (
            <div
              data-testid="cart-sidebar-empty"
              className="flex flex-col items-center justify-center py-16 text-center"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                width="48"
                height="48"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.5"
                strokeLinecap="round"
                strokeLinejoin="round"
                className="mb-4 text-muted-foreground/50"
              >
                <circle cx="8" cy="21" r="1" />
                <circle cx="19" cy="21" r="1" />
                <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
              </svg>
              <p className="text-muted-foreground">Your cart is empty</p>
            </div>
          ) : (
            <div className="divide-y">
              {items.map((item) => (
                <CartItemRow
                  key={item.id}
                  item={item}
                  onUpdateQuantity={updateQuantity}
                  onRemove={removeItem}
                />
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        {items.length > 0 && (
          <div className="border-t px-6 py-4">
            <div className="mb-4 flex items-center justify-between">
              <span className="text-sm text-muted-foreground">Total</span>
              <span
                data-testid="cart-sidebar-total"
                className="text-xl font-bold"
              >
                ${total.toFixed(2)}
              </span>
            </div>

            <button
              data-testid="cart-sidebar-checkout"
              className="w-full rounded-md bg-primary py-2.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
            >
              Checkout
            </button>

            <button
              data-testid="cart-sidebar-clear"
              onClick={clearCart}
              className="mt-2 w-full py-2 text-sm text-muted-foreground transition-colors hover:text-destructive"
            >
              Clear Cart
            </button>
          </div>
        )}
      </div>
    </>
  )
}
