import { describe, it, expect, beforeEach } from 'vitest';
import { useCartStore } from './cartStore';

const apple = { id: '1', name: 'Apple', price: 1.5 };
const banana = { id: '2', name: 'Banana', price: 0.75 };

describe('useCartStore', () => {
  beforeEach(() => {
    useCartStore.setState({ items: [] });
  });

  it('starts with an empty cart', () => {
    expect(useCartStore.getState().items).toEqual([]);
  });

  it('adds an item to the cart', () => {
    useCartStore.getState().addItem(apple);
    const items = useCartStore.getState().items;
    expect(items).toHaveLength(1);
    expect(items[0]).toEqual({ ...apple, quantity: 1 });
  });

  it('increments quantity when adding an existing item', () => {
    useCartStore.getState().addItem(apple);
    useCartStore.getState().addItem(apple);
    const items = useCartStore.getState().items;
    expect(items).toHaveLength(1);
    expect(items[0].quantity).toBe(2);
  });

  it('removes an item from the cart', () => {
    useCartStore.getState().addItem(apple);
    useCartStore.getState().addItem(banana);
    useCartStore.getState().removeItem('1');
    const items = useCartStore.getState().items;
    expect(items).toHaveLength(1);
    expect(items[0].id).toBe('2');
  });

  it('updates quantity of an item', () => {
    useCartStore.getState().addItem(apple);
    useCartStore.getState().updateQuantity('1', 5);
    expect(useCartStore.getState().items[0].quantity).toBe(5);
  });

  it('does not update quantity below 1', () => {
    useCartStore.getState().addItem(apple);
    useCartStore.getState().updateQuantity('1', 0);
    expect(useCartStore.getState().items[0].quantity).toBe(1);
  });

  it('calculates total correctly', () => {
    useCartStore.getState().addItem(apple); // 1.5
    useCartStore.getState().addItem(banana); // 0.75
    useCartStore.getState().updateQuantity('1', 3); // 3 * 1.5 = 4.5
    expect(useCartStore.getState().getTotal()).toBeCloseTo(5.25);
  });

  it('returns 0 total for empty cart', () => {
    expect(useCartStore.getState().getTotal()).toBe(0);
  });
});
