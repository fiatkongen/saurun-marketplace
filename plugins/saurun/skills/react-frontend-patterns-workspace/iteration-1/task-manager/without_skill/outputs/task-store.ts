import { create } from "zustand";
import { Task, TaskFilter } from "./types";

interface TaskState {
  tasks: Task[];
  filter: TaskFilter;
  isLoading: boolean;
  error: string | null;

  setTasks: (tasks: Task[]) => void;
  addTask: (task: Task) => void;
  toggleTask: (id: string) => void;
  setFilter: (filter: TaskFilter) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export const useTaskStore = create<TaskState>((set) => ({
  tasks: [],
  filter: "all",
  isLoading: false,
  error: null,

  setTasks: (tasks) => set({ tasks }),

  addTask: (task) =>
    set((state) => ({ tasks: [...state.tasks, task] })),

  toggleTask: (id) =>
    set((state) => ({
      tasks: state.tasks.map((task) =>
        task.id === id ? { ...task, completed: !task.completed } : task
      ),
    })),

  setFilter: (filter) => set({ filter }),
  setLoading: (isLoading) => set({ isLoading }),
  setError: (error) => set({ error }),
}));

export function useFilteredTasks(): Task[] {
  const tasks = useTaskStore((s) => s.tasks);
  const filter = useTaskStore((s) => s.filter);

  switch (filter) {
    case "active":
      return tasks.filter((t) => !t.completed);
    case "completed":
      return tasks.filter((t) => t.completed);
    default:
      return tasks;
  }
}
