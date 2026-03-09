// ============================================================================
// Product Catalog — Shared Types
// ============================================================================

export interface Product {
  id: string;
  title: string;
  price: number;
  category: string;
  rating: number;
  reviewCount: number;
  inStock: boolean;
  imageUrl?: string;
}

export interface FilterState {
  search: string;
  categories: string[];
  priceRange: [number, number];
  minRating: number;
  inStockOnly: boolean;
}

export type ViewMode = "grid" | "list";

export interface PaginationState {
  page: number;
  pageSize: number;
}

export interface CatalogUrlParams {
  search?: string;
  categories?: string;
  priceMin?: string;
  priceMax?: string;
  minRating?: string;
  inStock?: string;
  view?: ViewMode;
  page?: string;
  pageSize?: string;
}

export interface CartItem {
  product: Product;
  quantity: number;
}
