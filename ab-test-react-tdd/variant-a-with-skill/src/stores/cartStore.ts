import { create } from 'zustand'

export interface CartItem {
  id: string
  name: string
  price: number
  quantity: number
}

export interface CartStore {
  items: CartItem[]
  addItem: (product: { id: string; name: string; price: number }) => void
  removeItem: (id: string) => void
  updateQuantity: (id: string, quantity: number) => void
  getTotal: () => number
}

export const useCartStore = create<CartStore>((set, get) => ({
  items: [],

  addItem: (product) =>
    set((state) => {
      const existing = state.items.find((i) => i.id === product.id)
      if (existing) {
        return {
          items: state.items.map((i) =>
            i.id === product.id ? { ...i, quantity: i.quantity + 1 } : i
          ),
        }
      }
      return { items: [...state.items, { ...product, quantity: 1 }] }
    }),

  removeItem: (id) =>
    set((state) => ({
      items: state.items.filter((i) => i.id !== id),
    })),

  updateQuantity: (id, quantity) =>
    set((state) => {
      if (quantity <= 0) {
        return { items: state.items.filter((i) => i.id !== id) }
      }
      return {
        items: state.items.map((i) =>
          i.id === id ? { ...i, quantity } : i
        ),
      }
    }),

  getTotal: () => {
    return get().items.reduce((sum, item) => sum + item.price * item.quantity, 0)
  },
}))
