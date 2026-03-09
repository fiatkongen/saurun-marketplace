// =============================================================================
// ProductCard — Grid and list variants for product display
// =============================================================================

import { cn } from "./cn"
import { RatingStars } from "./RatingStars"
import { useCartActions } from "./useCartStore"
import type { Product } from "./types"

interface ProductCardProps {
  product: Product
  view: "grid" | "list"
}

export function ProductCard({ product, view }: ProductCardProps) {
  const { addItem } = useCartActions()

  const handleAddToCart = () => {
    addItem({
      id: product.id,
      title: product.title,
      price: product.price,
    })
  }

  if (view === "list") {
    return <ProductCardList product={product} onAddToCart={handleAddToCart} />
  }

  return <ProductCardGrid product={product} onAddToCart={handleAddToCart} />
}

// ============================================
// Grid View Card
// ============================================

function ProductCardGrid({
  product,
  onAddToCart,
}: {
  product: Product
  onAddToCart: () => void
}) {
  return (
    <article
      data-testid={`product-card-${product.id}`}
      className={cn(
        "group relative flex flex-col",
        "bg-white rounded-xl border border-stone-200/80",
        "transition-all duration-300",
        "hover:shadow-md hover:border-stone-300 hover:-translate-y-0.5"
      )}
    >
      {/* Image placeholder */}
      <div className="relative overflow-hidden rounded-t-xl">
        <div
          data-asset={`product-image-${product.id}`}
          className="aspect-[4/3] bg-gradient-to-br from-stone-100 to-stone-200"
        />
        {!product.inStock && (
          <div className="absolute inset-0 bg-white/60 flex items-center justify-center">
            <span className="text-xs font-bold uppercase tracking-wider text-stone-500 bg-white/80 px-3 py-1.5 rounded-full">
              Out of stock
            </span>
          </div>
        )}
        {/* Quick add overlay on hover */}
        <div
          className={cn(
            "absolute inset-x-0 bottom-0 p-3",
            "translate-y-full opacity-0 transition-all duration-300",
            "group-hover:translate-y-0 group-hover:opacity-100"
          )}
        >
          <button
            data-testid={`product-card-${product.id}-add-to-cart`}
            type="button"
            onClick={onAddToCart}
            disabled={!product.inStock}
            className={cn(
              "w-full py-2.5 rounded-lg text-sm font-semibold",
              "transition-colors duration-150",
              product.inStock
                ? "bg-stone-900 text-white hover:bg-stone-800 active:bg-stone-950"
                : "bg-stone-300 text-stone-500 cursor-not-allowed"
            )}
          >
            {product.inStock ? "Add to Cart" : "Unavailable"}
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="flex flex-col flex-1 p-4 gap-2">
        <p className="text-xs font-medium uppercase tracking-wider text-stone-400">
          {product.category}
        </p>
        <h3
          data-testid={`product-card-${product.id}-title`}
          className="text-sm font-semibold text-stone-900 leading-snug line-clamp-2"
        >
          {product.title}
        </h3>
        <div className="flex items-center gap-1.5 mt-auto pt-1">
          <RatingStars rating={product.rating} size="sm" />
          <span className="text-xs text-stone-400">
            ({product.reviewCount})
          </span>
        </div>
        <div className="flex items-center justify-between pt-1">
          <span
            data-testid={`product-card-${product.id}-price`}
            className="text-lg font-bold text-stone-900 tabular-nums"
          >
            ${product.price.toFixed(2)}
          </span>
          {product.inStock && (
            <span className="text-xs text-emerald-600 font-medium">In Stock</span>
          )}
        </div>
      </div>
    </article>
  )
}

// ============================================
// List View Card
// ============================================

function ProductCardList({
  product,
  onAddToCart,
}: {
  product: Product
  onAddToCart: () => void
}) {
  return (
    <article
      data-testid={`product-card-${product.id}`}
      className={cn(
        "group flex gap-5 p-4",
        "bg-white rounded-xl border border-stone-200/80",
        "transition-all duration-300",
        "hover:shadow-md hover:border-stone-300"
      )}
    >
      {/* Image placeholder */}
      <div className="relative shrink-0 w-40 overflow-hidden rounded-lg">
        <div
          data-asset={`product-image-${product.id}`}
          className="aspect-square bg-gradient-to-br from-stone-100 to-stone-200"
        />
        {!product.inStock && (
          <div className="absolute inset-0 bg-white/60 flex items-center justify-center">
            <span className="text-[10px] font-bold uppercase tracking-wider text-stone-500 bg-white/80 px-2 py-1 rounded-full">
              Out of stock
            </span>
          </div>
        )}
      </div>

      {/* Content */}
      <div className="flex flex-1 flex-col gap-1.5 min-w-0">
        <p className="text-xs font-medium uppercase tracking-wider text-stone-400">
          {product.category}
        </p>
        <h3
          data-testid={`product-card-${product.id}-title`}
          className="text-base font-semibold text-stone-900 leading-snug"
        >
          {product.title}
        </h3>
        <p className="text-sm text-stone-500 line-clamp-2">
          {product.description}
        </p>
        <div className="flex items-center gap-1.5 mt-1">
          <RatingStars rating={product.rating} size="sm" />
          <span className="text-xs text-stone-400">
            ({product.reviewCount})
          </span>
        </div>
      </div>

      {/* Price + Action */}
      <div className="flex flex-col items-end justify-between shrink-0 pl-4">
        <div className="text-right">
          <span
            data-testid={`product-card-${product.id}-price`}
            className="text-xl font-bold text-stone-900 tabular-nums"
          >
            ${product.price.toFixed(2)}
          </span>
          {product.inStock ? (
            <p className="text-xs text-emerald-600 font-medium mt-0.5">In Stock</p>
          ) : (
            <p className="text-xs text-stone-400 mt-0.5">Unavailable</p>
          )}
        </div>
        <button
          data-testid={`product-card-${product.id}-add-to-cart`}
          type="button"
          onClick={onAddToCart}
          disabled={!product.inStock}
          className={cn(
            "py-2 px-5 rounded-lg text-sm font-semibold",
            "transition-colors duration-150",
            product.inStock
              ? "bg-stone-900 text-white hover:bg-stone-800 active:bg-stone-950"
              : "bg-stone-200 text-stone-400 cursor-not-allowed"
          )}
        >
          {product.inStock ? "Add to Cart" : "Unavailable"}
        </button>
      </div>
    </article>
  )
}
