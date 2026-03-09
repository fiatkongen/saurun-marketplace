import { useCartStore } from '../store/cartStore';

export function CartPage() {
  const items = useCartStore((s) => s.items);
  const removeItem = useCartStore((s) => s.removeItem);
  const updateQuantity = useCartStore((s) => s.updateQuantity);
  const getTotal = useCartStore((s) => s.getTotal);

  if (items.length === 0) {
    return <p>Your cart is empty</p>;
  }

  return (
    <div>
      <h1>Shopping Cart</h1>
      <ul>
        {items.map((item) => (
          <li key={item.id} data-testid={`cart-item-${item.id}`}>
            <span>{item.name}</span>
            <span>${item.price.toFixed(2)}</span>
            <button
              aria-label={`Decrease quantity of ${item.name}`}
              onClick={() => updateQuantity(item.id, item.quantity - 1)}
            >
              -
            </button>
            <span aria-label={`Quantity of ${item.name}`}>{item.quantity}</span>
            <button
              aria-label={`Increase quantity of ${item.name}`}
              onClick={() => updateQuantity(item.id, item.quantity + 1)}
            >
              +
            </button>
            <button
              aria-label={`Remove ${item.name} from cart`}
              onClick={() => removeItem(item.id)}
            >
              Remove
            </button>
          </li>
        ))}
      </ul>
      <p data-testid="cart-total">Total: ${getTotal().toFixed(2)}</p>
    </div>
  );
}
