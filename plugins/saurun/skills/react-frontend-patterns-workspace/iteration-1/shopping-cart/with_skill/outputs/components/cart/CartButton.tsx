/**
 * CartButton
 *
 * Floating/header button that opens the cart sidebar.
 * Shows item count badge. Uses selector hooks -- never bare store.
 */

import { useCartCount, useCartSidebarActions } from '../../stores/useCartStore'

export function CartButton() {
  const count = useCartCount()
  const { toggleSidebar } = useCartSidebarActions()

  return (
    <button
      data-testid="cart-button"
      onClick={toggleSidebar}
      className="relative rounded-md p-2 transition-colors hover:bg-muted"
      aria-label={`Shopping cart with ${count} items`}
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        <circle cx="8" cy="21" r="1" />
        <circle cx="19" cy="21" r="1" />
        <path d="M2.05 2.05h2l2.66 12.42a2 2 0 0 0 2 1.58h9.78a2 2 0 0 0 1.95-1.57l1.65-7.43H5.12" />
      </svg>

      {count > 0 && (
        <span
          data-testid="cart-button-count"
          className="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary text-xs font-bold text-primary-foreground"
        >
          {count > 99 ? '99+' : count}
        </span>
      )}
    </button>
  )
}
