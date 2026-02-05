/**
 * Provider Setup
 *
 * App-level providers for React + TanStack Query + Error Boundaries.
 * Copy and adapt for your project.
 */

import { StrictMode, Suspense } from 'react'
import { ErrorBoundary, FallbackProps } from 'react-error-boundary'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { Toaster } from '@/components/ui/sonner'

// ============================================
// Query Client Configuration
// ============================================

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000, // 1 minute
      retry: 1,
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: 0,
    },
  },
})

// ============================================
// Error Fallback Components
// ============================================

function PageErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-4">
      <h1 className="text-2xl font-bold text-destructive">Something went wrong</h1>
      <p className="text-muted-foreground">{error.message}</p>
      <button
        onClick={resetErrorBoundary}
        className="rounded-sm bg-primary px-4 py-2 text-primary-foreground hover:bg-primary/90"
      >
        Try again
      </button>
    </div>
  )
}

function FeatureErrorFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="rounded-sm border border-destructive/50 bg-destructive/10 p-4">
      <p className="text-sm text-destructive">{error.message}</p>
      <button
        onClick={resetErrorBoundary}
        className="mt-2 text-xs text-muted-foreground underline"
      >
        Retry
      </button>
    </div>
  )
}

// ============================================
// Loading Fallbacks
// ============================================

function PageLoadingFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
    </div>
  )
}

// ============================================
// Root Provider
// ============================================

interface ProvidersProps {
  children: React.ReactNode
}

export function Providers({ children }: ProvidersProps) {
  return (
    <StrictMode>
      <QueryClientProvider client={queryClient}>
        <ErrorBoundary FallbackComponent={PageErrorFallback}>
          <Suspense fallback={<PageLoadingFallback />}>
            {children}
          </Suspense>
        </ErrorBoundary>
        <Toaster position="bottom-right" />
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </StrictMode>
  )
}

// ============================================
// Feature Error Boundary Wrapper
// ============================================

interface FeatureErrorBoundaryProps {
  children: React.ReactNode
  onReset?: () => void
}

export function FeatureErrorBoundary({
  children,
  onReset,
}: FeatureErrorBoundaryProps) {
  return (
    <ErrorBoundary
      FallbackComponent={FeatureErrorFallback}
      onReset={onReset}
    >
      {children}
    </ErrorBoundary>
  )
}

// ============================================
// Usage in main.tsx
// ============================================

/*
// main.tsx
import { createRoot } from 'react-dom/client'
import { Providers } from './providers'
import { App } from './App'

createRoot(document.getElementById('root')!).render(
  <Providers>
    <App />
  </Providers>
)
*/

// ============================================
// Usage in Page Components
// ============================================

/*
// pages/ProductsPage.tsx
import { FeatureErrorBoundary } from '@/providers'
import { ProductList } from '@/components/products/ProductList'
import { RecommendationsWidget } from '@/components/products/RecommendationsWidget'

export function ProductsPage() {
  return (
    <div className="container py-8">
      <h1 className="text-2xl font-bold mb-6">Products</h1>

      {// Main content - errors bubble to page boundary }
      <ProductList />

      {// Widget - errors isolated, doesn't crash page }
      <FeatureErrorBoundary>
        <RecommendationsWidget />
      </FeatureErrorBoundary>
    </div>
  )
}
*/

// ============================================
// Test Provider Wrapper
// ============================================

/*
// test-utils.tsx
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, RenderOptions } from '@testing-library/react'

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  })
}

interface TestProvidersProps {
  children: React.ReactNode
}

function TestProviders({ children }: TestProvidersProps) {
  const queryClient = createTestQueryClient()
  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  )
}

export function renderWithProviders(
  ui: React.ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) {
  return render(ui, { wrapper: TestProviders, ...options })
}

// Re-export everything
export * from '@testing-library/react'
export { renderWithProviders as render }
*/
