import { useTaskStore, useFilteredTasks } from "./task-store";
import { useTasksApi } from "./use-tasks-api";
import { CreateTaskForm } from "./CreateTaskForm";
import { TaskFilterBar } from "./TaskFilterBar";
import { TaskItem } from "./TaskItem";

export function TaskManagerPage() {
  const isLoading = useTaskStore((s) => s.isLoading);
  const error = useTaskStore((s) => s.error);
  const filteredTasks = useFilteredTasks();
  const { createTask, completeTask } = useTasksApi();

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Tasks</h1>

      <div className="space-y-4">
        <CreateTaskForm onSubmit={createTask} />

        <TaskFilterBar />

        {error && (
          <div className="rounded-md bg-red-50 border border-red-200 p-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {isLoading ? (
          <div className="text-center py-8 text-gray-400 text-sm">
            Loading tasks...
          </div>
        ) : filteredTasks.length === 0 ? (
          <div className="text-center py-8 text-gray-400 text-sm">
            No tasks found.
          </div>
        ) : (
          <ul className="space-y-2">
            {filteredTasks.map((task) => (
              <TaskItem
                key={task.id}
                task={task}
                onToggle={completeTask}
              />
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
