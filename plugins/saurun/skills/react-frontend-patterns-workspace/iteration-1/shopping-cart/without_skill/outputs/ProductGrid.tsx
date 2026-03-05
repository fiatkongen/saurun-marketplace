import React, { useState } from "react";
import { useProducts } from "./use-products";
import { ProductCard } from "./ProductCard";
import { CategoryFilter } from "./CategoryFilter";

export function ProductGrid() {
  const [selectedCategory, setSelectedCategory] = useState<
    string | undefined
  >();
  const { products, categories, isLoading, error } = useProducts({
    category: selectedCategory,
  });

  return (
    <div className="space-y-6">
      <CategoryFilter
        categories={categories}
        selected={selectedCategory}
        onSelect={setSelectedCategory}
      />

      {isLoading && (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div
              key={i}
              className="animate-pulse rounded-lg border border-gray-200 bg-white"
            >
              <div className="aspect-square rounded-t-lg bg-gray-200" />
              <div className="space-y-3 p-4">
                <div className="h-3 w-16 rounded bg-gray-200" />
                <div className="h-5 w-3/4 rounded bg-gray-200" />
                <div className="h-4 w-full rounded bg-gray-200" />
                <div className="flex items-center justify-between">
                  <div className="h-6 w-16 rounded bg-gray-200" />
                  <div className="h-9 w-24 rounded bg-gray-200" />
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700">
          <p className="font-medium">Failed to load products</p>
          <p className="mt-1 text-sm">{error}</p>
        </div>
      )}

      {!isLoading && !error && products.length === 0 && (
        <div className="py-12 text-center text-gray-500">
          <p className="text-lg">No products found</p>
          {selectedCategory && (
            <button
              onClick={() => setSelectedCategory(undefined)}
              className="mt-2 text-blue-600 hover:underline"
            >
              Clear filter
            </button>
          )}
        </div>
      )}

      {!isLoading && !error && products.length > 0 && (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {products.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </div>
  );
}
