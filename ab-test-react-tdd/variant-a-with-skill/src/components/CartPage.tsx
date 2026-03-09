import { useCartStore } from '../stores/cartStore'

export function CartPage() {
  const items = useCartStore((s) => s.items)
  const removeItem = useCartStore((s) => s.removeItem)
  const updateQuantity = useCartStore((s) => s.updateQuantity)
  const getTotal = useCartStore((s) => s.getTotal)

  if (items.length === 0) {
    return <p>Your cart is empty</p>
  }

  return (
    <div>
      <h1>Shopping Cart</h1>
      <ul>
        {items.map((item) => (
          <li key={item.id}>
            <span>{item.name}</span>
            <span>${item.price.toFixed(2)}</span>
            <button
              aria-label="-"
              onClick={() => updateQuantity(item.id, item.quantity - 1)}
            >
              -
            </button>
            <span>{item.quantity}</span>
            <button
              aria-label="+"
              onClick={() => updateQuantity(item.id, item.quantity + 1)}
            >
              +
            </button>
            <button onClick={() => removeItem(item.id)}>Remove</button>
          </li>
        ))}
      </ul>
      <p>${getTotal().toFixed(2)}</p>
    </div>
  )
}
