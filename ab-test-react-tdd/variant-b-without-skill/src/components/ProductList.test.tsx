import { describe, it, expect, beforeAll, afterAll, afterEach, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { server } from '../mocks/server';
import { ProductList } from './ProductList';
import { useCartStore } from '../store/cartStore';
import { http, HttpResponse } from 'msw';

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('ProductList', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] });
  });

  it('shows loading state initially', () => {
    render(<ProductList />);
    expect(screen.getByText('Loading products...')).toBeInTheDocument();
  });

  it('renders products from the API', async () => {
    render(<ProductList />);

    expect(await screen.findByText('Wireless Mouse')).toBeInTheDocument();
    expect(screen.getByText('Mechanical Keyboard')).toBeInTheDocument();
    expect(screen.getByText('USB-C Hub')).toBeInTheDocument();

    expect(screen.getByText('$29.99')).toBeInTheDocument();
    expect(screen.getByText('$89.99')).toBeInTheDocument();
    expect(screen.getByText('$49.99')).toBeInTheDocument();
  });

  it('adds a product to cart when "Add to Cart" is clicked', async () => {
    const user = userEvent.setup();
    render(<ProductList />);

    await screen.findByText('Wireless Mouse');

    const addButtons = screen.getAllByText('Add to Cart');
    await user.click(addButtons[0]);

    const items = useCartStore.getState().items;
    expect(items).toHaveLength(1);
    expect(items[0].name).toBe('Wireless Mouse');
    expect(items[0].quantity).toBe(1);
  });

  it('shows error state when API fails', async () => {
    server.use(
      http.get('/api/products', () => {
        return new HttpResponse(null, { status: 500 });
      })
    );

    render(<ProductList />);

    expect(
      await screen.findByText('Error: Failed to fetch products')
    ).toBeInTheDocument();
  });
});
