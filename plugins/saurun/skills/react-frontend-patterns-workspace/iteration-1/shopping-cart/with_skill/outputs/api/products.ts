import type { Product } from '../types'

// ============================================
// API Client
// ============================================

export const productsApi = {
  list: async (category?: string): Promise<Product[]> => {
    const params = category ? `?category=${category}` : ''
    const res = await fetch(`/api/products${params}`)
    if (!res.ok) throw new Error('Failed to fetch products')
    return res.json()
  },

  get: async (id: string): Promise<Product> => {
    const res = await fetch(`/api/products/${id}`)
    if (!res.ok) throw new Error('Product not found')
    return res.json()
  },
}
