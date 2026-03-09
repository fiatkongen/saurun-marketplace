// =============================================================================
// Product Catalog Types
// =============================================================================

export interface Product {
  id: string
  title: string
  price: number
  category: string
  rating: number
  reviewCount: number
  inStock: boolean
  description: string
}

export interface ProductFilters {
  search: string
  categories: string[]
  priceMin: number
  priceMax: number
  minRating: number
  inStockOnly: boolean
  page: number
  view: "grid" | "list"
  sortBy: "price-asc" | "price-desc" | "rating" | "name" | "newest"
}

export interface PaginatedResponse<T> {
  items: T[]
  totalItems: number
  totalPages: number
  currentPage: number
  pageSize: number
}

export interface CartItem {
  id: string
  title: string
  price: number
  quantity: number
}

export const CATEGORIES = [
  "Electronics",
  "Clothing",
  "Home & Garden",
  "Books",
  "Sports",
  "Toys",
  "Beauty",
  "Automotive",
] as const

export const PRICE_MIN = 0
export const PRICE_MAX = 1000
export const PAGE_SIZE = 12

export const DEFAULT_FILTERS: ProductFilters = {
  search: "",
  categories: [],
  priceMin: PRICE_MIN,
  priceMax: PRICE_MAX,
  minRating: 0,
  inStockOnly: false,
  page: 1,
  view: "grid",
  sortBy: "newest",
}
