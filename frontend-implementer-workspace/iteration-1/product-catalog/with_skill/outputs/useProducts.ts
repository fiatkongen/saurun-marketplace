// =============================================================================
// useProducts — TanStack Query hook for product fetching
// Query keys: ['products', 'list', filters]
// =============================================================================

import { useQuery } from "@tanstack/react-query"
import type { Product, ProductFilters, PaginatedResponse } from "./types"
import { PAGE_SIZE } from "./types"

// ============================================
// Query Key Factory
// ============================================

export const productKeys = {
  all: ["products"] as const,
  lists: () => [...productKeys.all, "list"] as const,
  list: (filters: Partial<ProductFilters>) =>
    [...productKeys.lists(), filters] as const,
}

// ============================================
// Mock data for demo — replace with real API
// ============================================

const MOCK_PRODUCTS: Product[] = Array.from({ length: 48 }, (_, i) => {
  const categories = [
    "Electronics",
    "Clothing",
    "Home & Garden",
    "Books",
    "Sports",
    "Toys",
    "Beauty",
    "Automotive",
  ]
  const titles = [
    "Wireless Headphones",
    "Leather Jacket",
    "Ceramic Planter",
    "Design Patterns",
    "Running Shoes",
    "Building Blocks",
    "Face Serum",
    "Car Vacuum",
    "Smart Watch",
    "Wool Sweater",
    "Table Lamp",
    "Sci-Fi Novel",
    "Yoga Mat",
    "Board Game",
    "Lip Balm",
    "Dash Cam",
  ]
  const category = categories[i % categories.length]
  const titleBase = titles[i % titles.length]
  return {
    id: `prod-${i + 1}`,
    title: `${titleBase} ${i + 1}`,
    price: Math.round((10 + Math.random() * 990) * 100) / 100,
    category,
    rating: Math.round((1 + Math.random() * 4) * 10) / 10,
    reviewCount: Math.floor(Math.random() * 500),
    inStock: Math.random() > 0.2,
    description: `High-quality ${titleBase.toLowerCase()} in the ${category.toLowerCase()} category. Expertly crafted for discerning customers.`,
  }
})

function filterAndPaginate(
  filters: ProductFilters
): PaginatedResponse<Product> {
  let filtered = [...MOCK_PRODUCTS]

  // Search
  if (filters.search) {
    const q = filters.search.toLowerCase()
    filtered = filtered.filter(
      (p) =>
        p.title.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q)
    )
  }

  // Categories
  if (filters.categories.length > 0) {
    filtered = filtered.filter((p) => filters.categories.includes(p.category))
  }

  // Price range
  filtered = filtered.filter(
    (p) => p.price >= filters.priceMin && p.price <= filters.priceMax
  )

  // Rating
  if (filters.minRating > 0) {
    filtered = filtered.filter((p) => p.rating >= filters.minRating)
  }

  // In-stock
  if (filters.inStockOnly) {
    filtered = filtered.filter((p) => p.inStock)
  }

  // Sort
  switch (filters.sortBy) {
    case "price-asc":
      filtered.sort((a, b) => a.price - b.price)
      break
    case "price-desc":
      filtered.sort((a, b) => b.price - a.price)
      break
    case "rating":
      filtered.sort((a, b) => b.rating - a.rating)
      break
    case "name":
      filtered.sort((a, b) => a.title.localeCompare(b.title))
      break
    case "newest":
    default:
      // Default order (by ID desc)
      filtered.sort(
        (a, b) =>
          parseInt(b.id.split("-")[1]) - parseInt(a.id.split("-")[1])
      )
  }

  // Paginate
  const totalItems = filtered.length
  const totalPages = Math.max(1, Math.ceil(totalItems / PAGE_SIZE))
  const page = Math.min(filters.page, totalPages)
  const start = (page - 1) * PAGE_SIZE
  const items = filtered.slice(start, start + PAGE_SIZE)

  return {
    items,
    totalItems,
    totalPages,
    currentPage: page,
    pageSize: PAGE_SIZE,
  }
}

// ============================================
// Query Hook
// ============================================

export function useProducts(filters: ProductFilters) {
  return useQuery({
    queryKey: productKeys.list(filters),
    queryFn: () => {
      // Simulate network delay
      return new Promise<PaginatedResponse<Product>>((resolve) => {
        setTimeout(() => {
          resolve(filterAndPaginate(filters))
        }, 200)
      })
    },
    staleTime: 60 * 1000,
    placeholderData: (previousData) => previousData,
  })
}
