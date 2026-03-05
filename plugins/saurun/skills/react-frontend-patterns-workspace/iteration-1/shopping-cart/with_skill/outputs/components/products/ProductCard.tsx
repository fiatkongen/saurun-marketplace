/**
 * ProductCard
 *
 * Displays a single product with add-to-cart action.
 * Uses selector hooks from cart store -- never bare useCartStore().
 */

import type { Product } from '../../types'
import { useCartActions } from '../../stores/useCartStore'

interface ProductCardProps {
  product: Product
}

export function ProductCard({ product }: ProductCardProps) {
  const { addItem } = useCartActions()

  function handleAddToCart() {
    addItem({
      id: product.id,
      name: product.name,
      price: product.price,
      imageUrl: product.imageUrl,
    })
  }

  return (
    <div
      data-testid={`product-card-${product.id}`}
      className="group flex flex-col overflow-hidden rounded-lg border bg-card shadow-sm transition-shadow hover:shadow-md"
    >
      <div className="aspect-square overflow-hidden bg-muted">
        <img
          src={product.imageUrl}
          alt={product.name}
          className="h-full w-full object-cover transition-transform group-hover:scale-105"
        />
      </div>

      <div className="flex flex-1 flex-col gap-2 p-4">
        <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
          {product.category}
        </p>
        <h3 className="font-semibold leading-tight">{product.name}</h3>
        <p className="text-sm text-muted-foreground line-clamp-2">
          {product.description}
        </p>

        <div className="mt-auto flex items-center justify-between pt-3">
          <span className="text-lg font-bold">
            ${product.price.toFixed(2)}
          </span>
          <button
            data-testid={`product-card-${product.id}-add-to-cart`}
            onClick={handleAddToCart}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90"
          >
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  )
}
