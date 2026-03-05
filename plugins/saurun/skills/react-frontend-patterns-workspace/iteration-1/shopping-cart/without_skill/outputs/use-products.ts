import { useEffect, useState } from "react";
import { Product } from "./types";

interface UseProductsOptions {
  category?: string;
}

interface UseProductsReturn {
  products: Product[];
  categories: string[];
  isLoading: boolean;
  error: string | null;
}

export function useProducts({
  category,
}: UseProductsOptions = {}): UseProductsReturn {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function fetchProducts() {
      setIsLoading(true);
      setError(null);

      try {
        const params = new URLSearchParams();
        if (category) params.set("category", category);

        const url = `/api/products${params.toString() ? `?${params}` : ""}`;
        const response = await fetch(url, { signal: controller.signal });

        if (!response.ok) {
          throw new Error(`Failed to fetch products: ${response.statusText}`);
        }

        const data: Product[] = await response.json();
        setProducts(data);

        // Extract unique categories from the full product list
        if (!category) {
          const uniqueCategories = [
            ...new Set(data.map((p) => p.category)),
          ].sort();
          setCategories(uniqueCategories);
        }
      } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") return;
        setError(
          err instanceof Error ? err.message : "An unknown error occurred"
        );
      } finally {
        setIsLoading(false);
      }
    }

    fetchProducts();
    return () => controller.abort();
  }, [category]);

  return { products, categories, isLoading, error };
}
