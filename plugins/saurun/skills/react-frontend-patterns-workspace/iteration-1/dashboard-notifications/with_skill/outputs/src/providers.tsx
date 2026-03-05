/**
 * Provider Setup
 *
 * App-level providers following the skill's provider-setup.tsx reference.
 * Includes ToastContainer for global notification rendering.
 */

import { StrictMode, Suspense } from 'react'
import { ErrorBoundary, FallbackProps } from 'react-error-boundary'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { ToastContainer } from '@/components/notifications/ToastContainer'

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
        data-testid="page-error-retry"
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
        data-testid="feature-error-retry"
      >
        Retry
      </button>
    </div>
  )
}

// ============================================
// Loading Fallback
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
        <ToastContainer />
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
