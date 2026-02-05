/**
 * TanStack Query Patterns
 *
 * Queries for reads. Mutations for writes.
 * Custom hooks wrap useQuery/useMutation.
 * Query keys follow [domain, action, params] pattern.
 */

import {
  useQuery,
  useMutation,
  useQueryClient,
  QueryClient,
} from '@tanstack/react-query'

// ============================================
// API Client (example)
// ============================================

interface Product {
  id: string
  name: string
  price: number
  category: string
}

interface CreateProductInput {
  name: string
  price: number
  category: string
}

// Your API client - adjust to your backend
const api = {
  products: {
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

    create: async (input: CreateProductInput): Promise<Product> => {
      const res = await fetch('/api/products', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(input),
      })
      if (!res.ok) throw new Error('Failed to create product')
      return res.json()
    },

    update: async (id: string, input: Partial<CreateProductInput>): Promise<Product> => {
      const res = await fetch(`/api/products/${id}`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(input),
      })
      if (!res.ok) throw new Error('Failed to update product')
      return res.json()
    },

    delete: async (id: string): Promise<void> => {
      const res = await fetch(`/api/products/${id}`, { method: 'DELETE' })
      if (!res.ok) throw new Error('Failed to delete product')
    },
  },
}

// ============================================
// Query Keys Factory
// ============================================

// Centralized query keys prevent typos and enable targeted invalidation
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
 * Fetch products list with optional category filter
 */
export function useProducts(category?: string) {
  return useQuery({
    queryKey: productKeys.list({ category }),
    queryFn: () => api.products.list(category),
    staleTime: 5 * 60 * 1000, // 5 minutes
  })
}

/**
 * Fetch single product by ID
 */
export function useProduct(id: string) {
  return useQuery({
    queryKey: productKeys.detail(id),
    queryFn: () => api.products.get(id),
    enabled: !!id, // Don't fetch if no ID
  })
}

// ============================================
// Mutation Hooks
// ============================================

/**
 * Create a new product
 */
export function useCreateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.products.create,
    onSuccess: () => {
      // Invalidate all product lists to refetch
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
    },
    onError: (error) => {
      // Handle error (e.g., show toast)
      console.error('Failed to create product:', error)
    },
  })
}

/**
 * Update existing product
 */
export function useUpdateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & Partial<CreateProductInput>) =>
      api.products.update(id, input),
    onSuccess: (data, variables) => {
      // Update the specific product in cache
      queryClient.setQueryData(productKeys.detail(variables.id), data)
      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
    },
  })
}

/**
 * Delete product
 */
export function useDeleteProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: api.products.delete,
    onSuccess: (_, id) => {
      // Remove from cache
      queryClient.removeQueries({ queryKey: productKeys.detail(id) })
      // Invalidate lists
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
    },
  })
}

// ============================================
// Usage in Components
// ============================================

/*
function ProductList({ category }: { category?: string }) {
  const { data: products, isPending, error } = useProducts(category)
  const createProduct = useCreateProduct()

  if (isPending) return <ProductListSkeleton />
  if (error) return <Alert variant="destructive">{error.message}</Alert>

  return (
    <div>
      {products.map((product) => (
        <ProductCard key={product.id} product={product} />
      ))}

      <Button
        onClick={() => createProduct.mutate({ name: 'New', price: 0, category: 'misc' })}
        disabled={createProduct.isPending}
      >
        {createProduct.isPending ? 'Creating...' : 'Add Product'}
      </Button>
    </div>
  )
}

function ProductDetail({ id }: { id: string }) {
  const { data: product, isPending, error } = useProduct(id)
  const updateProduct = useUpdateProduct()

  if (isPending) return <ProductDetailSkeleton />
  if (error) return <Alert variant="destructive">{error.message}</Alert>

  return (
    <div>
      <h1>{product.name}</h1>
      <Button
        onClick={() => updateProduct.mutate({ id, price: product.price + 1 })}
        disabled={updateProduct.isPending}
      >
        Increase Price
      </Button>
    </div>
  )
}
*/

// ============================================
// Optimistic Updates (Advanced)
// ============================================

/*
export function useOptimisticUpdateProduct() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, ...input }: { id: string } & Partial<CreateProductInput>) =>
      api.products.update(id, input),

    // Optimistically update before server response
    onMutate: async ({ id, ...input }) => {
      // Cancel outgoing refetches
      await queryClient.cancelQueries({ queryKey: productKeys.detail(id) })

      // Snapshot previous value
      const previousProduct = queryClient.getQueryData(productKeys.detail(id))

      // Optimistically update
      queryClient.setQueryData(productKeys.detail(id), (old: Product) => ({
        ...old,
        ...input,
      }))

      return { previousProduct }
    },

    // Rollback on error
    onError: (err, { id }, context) => {
      if (context?.previousProduct) {
        queryClient.setQueryData(productKeys.detail(id), context.previousProduct)
      }
    },

    // Refetch after success or error
    onSettled: (_, __, { id }) => {
      queryClient.invalidateQueries({ queryKey: productKeys.detail(id) })
    },
  })
}
*/

// ============================================
// Test Setup (with MSW)
// ============================================

/*
// In test file
import { http, HttpResponse } from 'msw'
import { server } from '@/mocks/server'

it('should fetch products', async () => {
  server.use(
    http.get('/api/products', () => {
      return HttpResponse.json([
        { id: '1', name: 'Widget', price: 9.99, category: 'tools' },
      ])
    })
  )

  render(<ProductList />)

  expect(await screen.findByText('Widget')).toBeInTheDocument()
})

it('should handle create error', async () => {
  server.use(
    http.post('/api/products', () => {
      return HttpResponse.json({ error: 'Failed' }, { status: 500 })
    })
  )

  render(<ProductList />)

  await userEvent.click(screen.getByRole('button', { name: /add/i }))

  expect(await screen.findByText(/failed/i)).toBeInTheDocument()
})
*/
