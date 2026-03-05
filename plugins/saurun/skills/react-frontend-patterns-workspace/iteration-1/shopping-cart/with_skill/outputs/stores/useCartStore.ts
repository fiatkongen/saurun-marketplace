/**
 * Cart Store
 *
 * One store for the cart domain. Actions own logic.
 * Always use exported selector hooks in components.
 * Reset for tests.
 */

import { create } from 'zustand'
import { useShallow } from 'zustand/react/shallow'
import { devtools } from 'zustand/middleware'
import type { CartItem } from '../types'

// ============================================
// Types
// ============================================

interface CartState {
  // State
  items: CartItem[]
  isOpen: boolean

  // Actions
  addItem: (item: Omit<CartItem, 'quantity'>) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  clearCart: () => void
  toggleSidebar: () => void
  openSidebar: () => void
  closeSidebar: () => void
  reset: () => void
}

// ============================================
// Initial State (for reset)
// ============================================

const initialState = {
  items: [] as CartItem[],
  isOpen: false,
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
            'addItem/increment'
          )
        } else {
          set(
            (state) => ({
              items: [...state.items, { ...item, quantity: 1 }],
            }),
            false,
            'addItem/new'
          )
        }
        // Auto-open sidebar when adding
        get().openSidebar()
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

      toggleSidebar: () => {
        set((state) => ({ isOpen: !state.isOpen }), false, 'toggleSidebar')
      },

      openSidebar: () => {
        set({ isOpen: true }, false, 'openSidebar')
      },

      closeSidebar: () => {
        set({ isOpen: false }, false, 'closeSidebar')
      },

      reset: () => {
        set(initialState, false, 'reset')
      },
    }),
    { name: 'cart-store' }
  )
)

// ============================================
// Selector Hooks (use these in components)
// ============================================

// Single field selectors
export const useCartItems = () => useCartStore((s) => s.items)
export const useCartIsOpen = () => useCartStore((s) => s.isOpen)

// Derived state (computed in selector, NOT stored)
export const useCartTotal = () =>
  useCartStore((s) =>
    s.items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  )

export const useCartCount = () =>
  useCartStore((s) => s.items.reduce((sum, item) => sum + item.quantity, 0))

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

export const useCartSidebarActions = () =>
  useCartStore(
    useShallow((s) => ({
      toggleSidebar: s.toggleSidebar,
      openSidebar: s.openSidebar,
      closeSidebar: s.closeSidebar,
    }))
  )
