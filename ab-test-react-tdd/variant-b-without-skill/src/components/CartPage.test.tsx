import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CartPage } from './CartPage';
import { useCartStore } from '../store/cartStore';

describe('CartPage', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] });
  });

  it('shows empty state when cart has no items', () => {
    render(<CartPage />);
    expect(screen.getByText('Your cart is empty')).toBeInTheDocument();
  });

  it('displays cart items with name, price, and quantity', () => {
    useCartStore.setState({
      items: [
        { id: '1', name: 'Widget', price: 9.99, quantity: 2 },
        { id: '2', name: 'Gadget', price: 19.99, quantity: 1 },
      ],
    });
    render(<CartPage />);

    expect(screen.getByText('Widget')).toBeInTheDocument();
    expect(screen.getByText('$9.99')).toBeInTheDocument();
    expect(screen.getByLabelText('Quantity of Widget')).toHaveTextContent('2');

    expect(screen.getByText('Gadget')).toBeInTheDocument();
    expect(screen.getByText('$19.99')).toBeInTheDocument();
    expect(screen.getByLabelText('Quantity of Gadget')).toHaveTextContent('1');
  });

  it('removes an item when Remove button is clicked', async () => {
    const user = userEvent.setup();
    useCartStore.setState({
      items: [{ id: '1', name: 'Widget', price: 9.99, quantity: 1 }],
    });
    render(<CartPage />);

    await user.click(screen.getByLabelText('Remove Widget from cart'));
    expect(screen.getByText('Your cart is empty')).toBeInTheDocument();
  });

  it('increments quantity when + is clicked', async () => {
    const user = userEvent.setup();
    useCartStore.setState({
      items: [{ id: '1', name: 'Widget', price: 9.99, quantity: 1 }],
    });
    render(<CartPage />);

    await user.click(screen.getByLabelText('Increase quantity of Widget'));
    expect(screen.getByLabelText('Quantity of Widget')).toHaveTextContent('2');
  });

  it('decrements quantity when - is clicked', async () => {
    const user = userEvent.setup();
    useCartStore.setState({
      items: [{ id: '1', name: 'Widget', price: 9.99, quantity: 3 }],
    });
    render(<CartPage />);

    await user.click(screen.getByLabelText('Decrease quantity of Widget'));
    expect(screen.getByLabelText('Quantity of Widget')).toHaveTextContent('2');
  });

  it('does not decrement quantity below 1', async () => {
    const user = userEvent.setup();
    useCartStore.setState({
      items: [{ id: '1', name: 'Widget', price: 9.99, quantity: 1 }],
    });
    render(<CartPage />);

    await user.click(screen.getByLabelText('Decrease quantity of Widget'));
    expect(screen.getByLabelText('Quantity of Widget')).toHaveTextContent('1');
  });

  it('displays the correct cart total', () => {
    useCartStore.setState({
      items: [
        { id: '1', name: 'Widget', price: 10.0, quantity: 2 },
        { id: '2', name: 'Gadget', price: 5.5, quantity: 3 },
      ],
    });
    render(<CartPage />);

    // 2*10 + 3*5.5 = 36.50
    expect(screen.getByTestId('cart-total')).toHaveTextContent('Total: $36.50');
  });
});
