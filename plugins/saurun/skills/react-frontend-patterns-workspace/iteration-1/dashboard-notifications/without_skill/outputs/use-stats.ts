// ============================================================
// use-stats.ts — Hook for fetching user statistics
// ============================================================

import { useState, useEffect } from "react";
import type { UserStats } from "./types";
import { toast } from "./toast-store";

interface UseStatsReturn {
  stats: UserStats | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export function useStats(): UseStatsReturn {
  const [stats, setStats] = useState<UserStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStats = async () => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await fetch("/api/stats");

      if (!response.ok) {
        throw new Error(`Failed to fetch stats: ${response.status}`);
      }

      const data: UserStats = await response.json();
      setStats(data);
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unknown error";
      setError(message);
      toast.error(`Failed to load statistics: ${message}`);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchStats();
  }, []);

  return { stats, isLoading, error, refetch: fetchStats };
}
