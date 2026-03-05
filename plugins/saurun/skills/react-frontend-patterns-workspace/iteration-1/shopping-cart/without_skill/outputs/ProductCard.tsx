import React from "react";
import { Product } from "./types";
import { useCartStore } from "./cart-store";

interface ProductCardProps {
  product: Product;
}

export function ProductCard({ product }: ProductCardProps) {
  const addItem = useCartStore((state) => state.addItem);
  const openCart = useCartStore((state) => state.openCart);

  function handleAddToCart() {
    addItem(product);
    openCart();
  }

  return (
    <div className="group rounded-lg border border-gray-200 bg-white shadow-sm transition-shadow hover:shadow-md">
      <div className="aspect-square overflow-hidden rounded-t-lg bg-gray-100">
        <img
          src={product.image}
          alt={product.name}
          className="h-full w-full object-cover transition-transform group-hover:scale-105"
        />
      </div>
      <div className="p-4">
        <span className="text-xs font-medium uppercase tracking-wide text-gray-500">
          {product.category}
        </span>
        <h3 className="mt-1 text-lg font-semibold text-gray-900">
          {product.name}
        </h3>
        <p className="mt-1 line-clamp-2 text-sm text-gray-600">
          {product.description}
        </p>
        <div className="mt-4 flex items-center justify-between">
          <span className="text-xl font-bold text-gray-900">
            ${product.price.toFixed(2)}
          </span>
          <button
            onClick={handleAddToCart}
            className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-blue-700 active:bg-blue-800"
          >
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  );
}
