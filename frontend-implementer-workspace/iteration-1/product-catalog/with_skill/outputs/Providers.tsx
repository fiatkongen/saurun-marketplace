// =============================================================================
// Providers — App-level provider stack
// QueryClient + ErrorBoundary + Router
// =============================================================================

import { StrictMode, Suspense } from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { BrowserRouter } from "react-router-dom"

// ============================================
// Query Client
// ============================================

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
    mutations: {
      retry: 0,
    },
  },
})

// ============================================
// Loading Fallback
// ============================================

function PageLoadingFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-stone-50">
      <div className="h-8 w-8 animate-spin rounded-full border-4 border-stone-300 border-t-amber-600" />
    </div>
  )
}

// ============================================
// Error Fallback
// ============================================

function PageErrorFallback({
  error,
  resetErrorBoundary,
}: {
  error: Error
  resetErrorBoundary: () => void
}) {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-4 bg-stone-50">
      <h1 className="text-2xl font-bold text-red-600">Something went wrong</h1>
      <p className="text-stone-500">{error.message}</p>
      <button
        onClick={resetErrorBoundary}
        className="rounded-lg bg-stone-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-stone-800"
      >
        Try again
      </button>
    </div>
  )
}

// ============================================
// Root Providers
// ============================================

interface ProvidersProps {
  children: React.ReactNode
}

export function Providers({ children }: ProvidersProps) {
  return (
    <StrictMode>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <Suspense fallback={<PageLoadingFallback />}>
            {children}
          </Suspense>
        </QueryClientProvider>
      </BrowserRouter>
    </StrictMode>
  )
}

// ============================================
// Test Providers (no StrictMode, MemoryRouter)
// ============================================

export function TestProviders({ children }: { children: React.ReactNode }) {
  const testClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  })

  return (
    <QueryClientProvider client={testClient}>
      {children}
    </QueryClientProvider>
  )
}
