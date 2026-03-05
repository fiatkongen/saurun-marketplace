/**
 * CartItemRow
 *
 * Single item in the cart sidebar with quantity controls.
 * Events flow up via callbacks -- no direct store access here.
 */

import type { CartItem } from '../../types'

interface CartItemRowProps {
  item: CartItem
  onUpdateQuantity: (id: string, quantity: number) => void
  onRemove: (id: string) => void
}

export function CartItemRow({
  item,
  onUpdateQuantity,
  onRemove,
}: CartItemRowProps) {
  return (
    <div
      data-testid={`cart-item-${item.id}`}
      className="flex gap-3 border-b py-3 last:border-b-0"
    >
      <div className="h-16 w-16 shrink-0 overflow-hidden rounded-md bg-muted">
        <img
          src={item.imageUrl}
          alt={item.name}
          className="h-full w-full object-cover"
        />
      </div>

      <div className="flex flex-1 flex-col justify-between">
        <div className="flex items-start justify-between">
          <h4 className="text-sm font-medium leading-tight">{item.name}</h4>
          <button
            data-testid={`cart-item-${item.id}-remove`}
            onClick={() => onRemove(item.id)}
            className="ml-2 text-muted-foreground transition-colors hover:text-destructive"
            aria-label={`Remove ${item.name}`}
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
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

        <div className="flex items-center justify-between">
          <div className="flex items-center gap-1">
            <button
              data-testid={`cart-item-${item.id}-decrement`}
              onClick={() => onUpdateQuantity(item.id, item.quantity - 1)}
              className="flex h-6 w-6 items-center justify-center rounded border text-sm transition-colors hover:bg-muted"
              aria-label={`Decrease quantity of ${item.name}`}
            >
              -
            </button>
            <span
              data-testid={`cart-item-${item.id}-quantity`}
              className="w-8 text-center text-sm font-medium"
            >
              {item.quantity}
            </span>
            <button
              data-testid={`cart-item-${item.id}-increment`}
              onClick={() => onUpdateQuantity(item.id, item.quantity + 1)}
              className="flex h-6 w-6 items-center justify-center rounded border text-sm transition-colors hover:bg-muted"
              aria-label={`Increase quantity of ${item.name}`}
            >
              +
            </button>
          </div>
          <span className="text-sm font-semibold">
            ${(item.price * item.quantity).toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  )
}
