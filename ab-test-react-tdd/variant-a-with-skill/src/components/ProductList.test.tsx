import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen } from '../test/test-utils'
import userEvent from '@testing-library/user-event'
import { ProductList } from './ProductList'
import { useCartStore } from '../stores/cartStore'

describe('ProductList', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] })
  })

  it('should show loading state initially', () => {
    render(<ProductList />)
    expect(screen.getByText(/loading/i)).toBeInTheDocument()
  })

  it('should display products fetched from API', async () => {
    render(<ProductList />)
    expect(await screen.findByText('Wireless Mouse')).toBeInTheDocument()
    expect(screen.getByText('Mechanical Keyboard')).toBeInTheDocument()
    expect(screen.getByText('USB-C Hub')).toBeInTheDocument()
  })

  it('should add product to cart when Add to Cart is clicked', async () => {
    const user = userEvent.setup()
    render(<ProductList />)
    const buttons = await screen.findAllByRole('button', { name: /add to cart/i })
    await user.click(buttons[0])
    expect(useCartStore.getState().items).toHaveLength(1)
    expect(useCartStore.getState().items[0].name).toBe('Wireless Mouse')
  })
})
