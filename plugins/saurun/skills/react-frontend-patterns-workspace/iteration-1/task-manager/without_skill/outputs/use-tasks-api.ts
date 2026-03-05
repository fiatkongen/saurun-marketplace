import { useCallback, useEffect } from "react";
import { useTaskStore } from "./task-store";
import { CreateTaskRequest, Task, TasksResponse } from "./types";

const API_BASE = "/api/tasks";

async function fetchJson<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });
  if (!res.ok) {
    throw new Error(`API error: ${res.status} ${res.statusText}`);
  }
  return res.json();
}

export function useTasksApi() {
  const { setTasks, addTask, toggleTask, setLoading, setError } =
    useTaskStore();

  const loadTasks = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchJson<TasksResponse>(API_BASE);
      setTasks(data.tasks);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load tasks");
    } finally {
      setLoading(false);
    }
  }, [setTasks, setLoading, setError]);

  const createTask = useCallback(
    async (title: string) => {
      setError(null);
      try {
        const task = await fetchJson<Task>(API_BASE, {
          method: "POST",
          body: JSON.stringify({ title } satisfies CreateTaskRequest),
        });
        addTask(task);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : "Failed to create task"
        );
      }
    },
    [addTask, setError]
  );

  const completeTask = useCallback(
    async (id: string) => {
      // Optimistic update
      toggleTask(id);
      try {
        await fetchJson<Task>(`${API_BASE}/${id}/toggle`, {
          method: "PATCH",
        });
      } catch (err) {
        // Revert on failure
        toggleTask(id);
        setError(
          err instanceof Error ? err.message : "Failed to update task"
        );
      }
    },
    [toggleTask, setError]
  );

  // Load tasks on mount
  useEffect(() => {
    loadTasks();
  }, [loadTasks]);

  return { loadTasks, createTask, completeTask };
}
