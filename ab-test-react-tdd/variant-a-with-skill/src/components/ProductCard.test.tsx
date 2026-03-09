import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '../test/test-utils'
import userEvent from '@testing-library/user-event'
import { ProductCard } from './ProductCard'
import { useCartStore } from '../stores/cartStore'

describe('ProductCard', () => {
  const product = { id: '1', name: 'Wireless Mouse', price: 29.99 }

  beforeEach(() => {
    useCartStore.setState({ items: [] })
  })

  it('should display product name and price', () => {
    render(<ProductCard product={product} />)
    expect(screen.getByText('Wireless Mouse')).toBeInTheDocument()
    expect(screen.getByText('$29.99')).toBeInTheDocument()
  })

  it('should add product to cart when Add to Cart button is clicked', async () => {
    const user = userEvent.setup()
    render(<ProductCard product={product} />)
    await user.click(screen.getByRole('button', { name: /add to cart/i }))
    expect(useCartStore.getState().items).toHaveLength(1)
    expect(useCartStore.getState().items[0].name).toBe('Wireless Mouse')
  })

  it('should increment quantity when Add to Cart is clicked twice', async () => {
    const user = userEvent.setup()
    render(<ProductCard product={product} />)
    await user.click(screen.getByRole('button', { name: /add to cart/i }))
    await user.click(screen.getByRole('button', { name: /add to cart/i }))
    expect(useCartStore.getState().items[0].quantity).toBe(2)
  })
})
