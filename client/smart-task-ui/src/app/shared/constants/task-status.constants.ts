import { TaskItemStatus } from '../../core/models/enums';

export const TASK_STATUS_LABELS = {
  [TaskItemStatus.ToDo]: 'To Do',
  [TaskItemStatus.InProgress]: 'In Progress',
  [TaskItemStatus.Completed]: 'Completed',
  [TaskItemStatus.Cancelled]: 'Cancelled',
} as const;

export const TASK_STATUS_COLORS = {
  [TaskItemStatus.ToDo]: 'primary',
  [TaskItemStatus.InProgress]: 'accent',
  [TaskItemStatus.Completed]: 'success',
  [TaskItemStatus.Cancelled]: 'warn',
} as const;
