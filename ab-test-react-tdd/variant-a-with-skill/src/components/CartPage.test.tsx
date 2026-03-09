import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '../test/test-utils'
import userEvent from '@testing-library/user-event'
import { CartPage } from './CartPage'
import { useCartStore } from '../stores/cartStore'

describe('CartPage', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] })
  })

  it('should show empty cart message when cart has no items', () => {
    render(<CartPage />)
    expect(screen.getByText(/your cart is empty/i)).toBeInTheDocument()
  })

  it('should display item name, price, and quantity when cart has items', () => {
    useCartStore.setState({
      items: [{ id: '1', name: 'Mouse', price: 29.99, quantity: 2 }],
    })
    render(<CartPage />)
    expect(screen.getByText('Mouse')).toBeInTheDocument()
    expect(screen.getByText('$29.99')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('should remove item when remove button is clicked', async () => {
    const user = userEvent.setup()
    useCartStore.setState({
      items: [{ id: '1', name: 'Mouse', price: 29.99, quantity: 1 }],
    })
    render(<CartPage />)
    await user.click(screen.getByRole('button', { name: /remove/i }))
    expect(screen.getByText(/your cart is empty/i)).toBeInTheDocument()
  })

  it('should increment quantity when + button is clicked', async () => {
    const user = userEvent.setup()
    useCartStore.setState({
      items: [{ id: '1', name: 'Mouse', price: 29.99, quantity: 1 }],
    })
    render(<CartPage />)
    await user.click(screen.getByRole('button', { name: '+' }))
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('should decrement quantity when - button is clicked', async () => {
    const user = userEvent.setup()
    useCartStore.setState({
      items: [{ id: '1', name: 'Mouse', price: 29.99, quantity: 3 }],
    })
    render(<CartPage />)
    await user.click(screen.getByRole('button', { name: '-' }))
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('should remove item when - is clicked at quantity 1', async () => {
    const user = userEvent.setup()
    useCartStore.setState({
      items: [{ id: '1', name: 'Mouse', price: 29.99, quantity: 1 }],
    })
    render(<CartPage />)
    await user.click(screen.getByRole('button', { name: '-' }))
    expect(screen.getByText(/your cart is empty/i)).toBeInTheDocument()
  })

  it('should display the cart total', () => {
    useCartStore.setState({
      items: [
        { id: '1', name: 'Mouse', price: 10, quantity: 2 },
        { id: '2', name: 'Keyboard', price: 20, quantity: 1 },
      ],
    })
    render(<CartPage />)
    expect(screen.getByText('$40.00')).toBeInTheDocument()
  })
})
