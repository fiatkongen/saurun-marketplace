import { Task } from "./types";

interface TaskItemProps {
  task: Task;
  onToggle: (id: string) => void;
}

export function TaskItem({ task, onToggle }: TaskItemProps) {
  return (
    <li className="flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:border-gray-300 transition-colors">
      <input
        type="checkbox"
        checked={task.completed}
        onChange={() => onToggle(task.id)}
        className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
      />
      <span
        className={`flex-1 text-sm ${
          task.completed ? "line-through text-gray-400" : "text-gray-900"
        }`}
      >
        {task.title}
      </span>
      <time className="text-xs text-gray-400">
        {new Date(task.createdAt).toLocaleDateString()}
      </time>
    </li>
  );
}
