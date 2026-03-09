import { useCartStore } from '../stores/cartStore'

interface Product {
  id: string
  name: string
  price: number
}

export function ProductCard({ product }: { product: Product }) {
  const addItem = useCartStore((s) => s.addItem)

  return (
    <div>
      <h2>{product.name}</h2>
      <p>${product.price.toFixed(2)}</p>
      <button onClick={() => addItem(product)}>Add to Cart</button>
    </div>
  )
}
