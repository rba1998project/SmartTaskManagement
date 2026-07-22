import { TaskItemStatus, TaskItemPriority } from './enums';

export interface DashboardResponse {
  totalProjects: number;
  totalTasks: number;
  tasksByStatus: Record<TaskItemStatus, number>;
  tasksByPriority: Record<TaskItemPriority, number>;
  completedTasks: number;
  pendingTasks: number;
  upcomingDueTasks: number;
}
