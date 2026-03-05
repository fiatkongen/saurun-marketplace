export interface Task {
  id: string;
  title: string;
  completed: boolean;
  createdAt: string;
}

export type TaskFilter = "all" | "active" | "completed";

export interface CreateTaskRequest {
  title: string;
}

export interface TasksResponse {
  tasks: Task[];
}
