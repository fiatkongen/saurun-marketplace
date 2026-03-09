import { useCartStore } from '../store/cartStore';
import type { Product } from '../types';

interface ProductCardProps {
  product: Product;
}

export function ProductCard({ product }: ProductCardProps) {
  const addItem = useCartStore((s) => s.addItem);

  return (
    <div data-testid={`product-${product.id}`}>
      <h2>{product.name}</h2>
      <p>${product.price.toFixed(2)}</p>
      <button onClick={() => addItem(product)}>Add to Cart</button>
    </div>
  );
}
