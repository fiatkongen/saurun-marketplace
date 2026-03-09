import { describe, it, expect, beforeEach } from 'vitest'
import { useCartStore } from './cartStore'

describe('useCartStore', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] })
  })

  it('should start with an empty items array', () => {
    const { items } = useCartStore.getState()
    expect(items).toEqual([])
  })

  it('should add an item to the cart when addItem is called', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    const { items } = useCartStore.getState()
    expect(items).toHaveLength(1)
    expect(items[0]).toEqual({ id: '1', name: 'Mouse', price: 29.99, quantity: 1 })
  })

  it('should increment quantity when adding an existing item', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    const { items } = useCartStore.getState()
    expect(items).toHaveLength(1)
    expect(items[0].quantity).toBe(2)
  })

  it('should remove an item when removeItem is called', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    useCartStore.getState().removeItem('1')
    expect(useCartStore.getState().items).toHaveLength(0)
  })

  it('should update quantity when updateQuantity is called', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    useCartStore.getState().updateQuantity('1', 5)
    expect(useCartStore.getState().items[0].quantity).toBe(5)
  })

  it('should remove item when updateQuantity is called with 0', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 29.99 })
    useCartStore.getState().updateQuantity('1', 0)
    expect(useCartStore.getState().items).toHaveLength(0)
  })

  it('should calculate total price with getTotal', () => {
    useCartStore.getState().addItem({ id: '1', name: 'Mouse', price: 10 })
    useCartStore.getState().addItem({ id: '2', name: 'Keyboard', price: 20 })
    useCartStore.getState().updateQuantity('1', 3)
    expect(useCartStore.getState().getTotal()).toBe(50) // 10*3 + 20*1
  })
})
