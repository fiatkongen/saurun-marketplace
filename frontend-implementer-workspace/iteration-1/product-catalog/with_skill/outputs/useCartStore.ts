// =============================================================================
// Cart Store — Zustand
// One store for cart domain. Actions own logic. Selectors exported.
// =============================================================================

import { create } from "zustand"
import { useShallow } from "zustand/react/shallow"
import { devtools } from "zustand/middleware"
import type { CartItem } from "./types"

// ============================================
// State Shape
// ============================================

interface CartState {
  items: CartItem[]

  addItem: (item: Omit<CartItem, "quantity">) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  reset: () => void
}

const initialState = {
  items: [] as CartItem[],
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
          set(
            (state) => ({
              items: state.items.map((i) =>
                i.id === item.id ? { ...i, quantity: i.quantity + 1 } : i
              ),
            }),
            false,
            "addItem/increment"
          )
        } else {
          set(
            (state) => ({
              items: [...state.items, { ...item, quantity: 1 }],
            }),
            false,
            "addItem/new"
          )
        }
      },

      removeItem: (id) => {
        set(
          (state) => ({
            items: state.items.filter((i) => i.id !== id),
          }),
          false,
          "removeItem"
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
          "updateQuantity"
        )
      },

      clearCart: () => {
        set({ items: [] }, false, "clearCart")
      },

      reset: () => {
        set(initialState, false, "reset")
      },
    }),
    { name: "cart-store" }
  )
)

// ============================================
// Selectors
// ============================================

export const useCartItems = () => useCartStore((s) => s.items)

export const useCartActions = () =>
  useCartStore(
    useShallow((s) => ({
      addItem: s.addItem,
      removeItem: s.removeItem,
      updateQuantity: s.updateQuantity,
      clearCart: s.clearCart,
    }))
  )

/** Derived: total number of items in cart (sum of quantities) */
export const useCartCount = () =>
  useCartStore((s) => s.items.reduce((sum, item) => sum + item.quantity, 0))

/** Derived: total price */
export const useCartTotal = () =>
  useCartStore((s) =>
    s.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  )
