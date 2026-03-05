/**
 * Product Query Hooks
 *
 * TanStack Query hooks for product data.
 * Query keys follow [domain, action, params] pattern.
 * Server data lives in TanStack Query cache -- never Zustand.
 */

import { useQuery } from '@tanstack/react-query'
import { productsApi } from '../api/products'

// ============================================
// Query Keys Factory
// ============================================

export const productKeys = {
  all: ['products'] as const,
  lists: () => [...productKeys.all, 'list'] as const,
  list: (filters: { category?: string }) =>
    [...productKeys.lists(), filters] as const,
  details: () => [...productKeys.all, 'detail'] as const,
  detail: (id: string) => [...productKeys.details(), id] as const,
}

// ============================================
// Query Hooks
// ============================================

/**
 * Fetch products with optional category filter.
 * Category param included in query key for automatic refetch on change.
 */
export function useProducts(category?: string) {
  return useQuery({
    queryKey: productKeys.list({ category }),
    queryFn: () => productsApi.list(category),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

/**
 * Fetch single product by ID.
 */
export function useProduct(id: string) {
  return useQuery({
    queryKey: productKeys.detail(id),
    queryFn: () => productsApi.get(id),
    enabled: !!id,
  })
}
