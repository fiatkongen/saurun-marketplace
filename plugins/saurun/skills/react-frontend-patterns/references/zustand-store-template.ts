/**
 * Zustand Store Template
 *
 * One store per feature domain. Actions own async logic.
 * Always use selectors. Reset for tests.
 */

import { create } from 'zustand'
import { useShallow } from 'zustand/react/shallow'
import { devtools } from 'zustand/middleware'

// ============================================
// Types
// ============================================

interface CartItem {
  id: string
  name: string
  price: number
  quantity: number
}

interface CartState {
  // State
  items: CartItem[]
  isLoading: boolean
  error: string | null

  // Actions
  addItem: (item: Omit<CartItem, 'quantity'>) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  reset: () => void
}

// ============================================
// Initial State (for reset)
// ============================================

const initialState = {
  items: [] as CartItem[],
  isLoading: false,
  error: null as string | null,
}

// ============================================
// Store
// ============================================

export const useCartStore = create<CartState>()(
  devtools(
    (set, get) => ({
      ...initialState,

      addItem: (item) => {
        const existing = get().items.find((i) => i.id === item.id)
        if (existing) {
          // Increment quantity
          set(
            (state) => ({
              items: state.items.map((i) =>
                i.id === item.id ? { ...i, quantity: i.quantity + 1 } : i
              ),
            }),
            false,
            'addItem/increment'
          )
        } else {
          // Add new item
          set(
            (state) => ({
              items: [...state.items, { ...item, quantity: 1 }],
            }),
            false,
            'addItem/new'
          )
        }
      },

      removeItem: (id) => {
        set(
          (state) => ({
            items: state.items.filter((i) => i.id !== id),
          }),
          false,
          'removeItem'
        )
      },

      updateQuantity: (id, quantity) => {
        if (quantity <= 0) {
          get().removeItem(id)
          return
        }
        set(
          (state) => ({
            items: state.items.map((i) =>
              i.id === id ? { ...i, quantity } : i
            ),
          }),
          false,
          'updateQuantity'
        )
      },

      clearCart: () => {
        set({ items: [] }, false, 'clearCart')
      },

      reset: () => {
        set(initialState, false, 'reset')
      },
    }),
    { name: 'cart-store' }
  )
)

// ============================================
// Selectors (use these in components)
// ============================================

// Single field selectors
export const useCartItems = () => useCartStore((s) => s.items)
export const useCartLoading = () => useCartStore((s) => s.isLoading)
export const useCartError = () => useCartStore((s) => s.error)

// Action selectors
export const useCartActions = () =>
  useCartStore(
    useShallow((s) => ({
      addItem: s.addItem,
      removeItem: s.removeItem,
      updateQuantity: s.updateQuantity,
      clearCart: s.clearCart,
    }))
  )

// Derived state (computed in selector, NOT stored)
export const useCartTotal = () =>
  useCartStore((s) =>
    s.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  )

export const useCartCount = () =>
  useCartStore((s) => s.items.reduce((sum, item) => sum + item.quantity, 0))

// ============================================
// Usage in Components
// ============================================

/*
function CartSummary() {
  // ✅ Good: individual selectors
  const items = useCartItems()
  const total = useCartTotal()
  const { removeItem } = useCartActions()

  return (
    <div>
      {items.map((item) => (
        <CartItem
          key={item.id}
          item={item}
          onRemove={() => removeItem(item.id)}
        />
      ))}
      <div>Total: ${total.toFixed(2)}</div>
    </div>
  )
}

// ❌ Bad: bare store causes re-render on ANY change
function BadComponent() {
  const store = useCartStore() // Never do this
  return <div>{store.items.length}</div>
}
*/

// ============================================
// Test Setup
// ============================================

/*
// In test file or vitest.setup.ts
import { beforeEach } from 'vitest'
import { useCartStore } from '@/stores/useCartStore'

beforeEach(() => {
  useCartStore.getState().reset()
})

// Or for specific test state:
beforeEach(() => {
  useCartStore.setState({
    items: [{ id: '1', name: 'Test Item', price: 10, quantity: 2 }],
    isLoading: false,
    error: null,
  })
})
*/
