// ============================================================
// DashboardPage.tsx — Main dashboard page composing all sections
// ============================================================

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StatCardsRow } from "./StatCardsRow";
import { ActivityFeed } from "./ActivityFeed";
import { FilterBar } from "./FilterBar";

// ---------------------------------------------------------------------------
// Query client instance (would normally live in app root)
// ---------------------------------------------------------------------------

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 2,
      refetchOnWindowFocus: false,
    },
  },
});

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

function DashboardContent() {
  return (
    <div className="mx-auto max-w-7xl space-y-8 p-6 lg:p-8">
      {/* Page header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="mt-1 text-muted-foreground">
          Overview of your platform activity and key metrics.
        </p>
      </div>

      {/* Stat cards */}
      <StatCardsRow />

      {/* Activity section */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-xl font-semibold tracking-tight">Recent Activity</h2>
        </div>

        {/* Filter bar */}
        <FilterBar />

        {/* Activity feed with infinite scroll */}
        <ActivityFeed />
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Exported page with providers
// ---------------------------------------------------------------------------

export function DashboardPage() {
  return (
    <QueryClientProvider client={queryClient}>
      <DashboardContent />
    </QueryClientProvider>
  );
}

export default DashboardPage;
