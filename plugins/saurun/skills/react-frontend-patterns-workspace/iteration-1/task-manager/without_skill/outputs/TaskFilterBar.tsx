import { useTaskStore } from "./task-store";
import { TaskFilter } from "./types";

const FILTERS: { value: TaskFilter; label: string }[] = [
  { value: "all", label: "All" },
  { value: "active", label: "Active" },
  { value: "completed", label: "Completed" },
];

export function TaskFilterBar() {
  const filter = useTaskStore((s) => s.filter);
  const setFilter = useTaskStore((s) => s.setFilter);

  return (
    <div className="flex gap-2">
      {FILTERS.map(({ value, label }) => (
        <button
          key={value}
          onClick={() => setFilter(value)}
          className={`px-3 py-1.5 rounded-md text-sm font-medium transition-colors ${
            filter === value
              ? "bg-blue-600 text-white"
              : "bg-gray-100 text-gray-700 hover:bg-gray-200"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
