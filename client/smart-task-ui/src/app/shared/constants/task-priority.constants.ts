import { TaskItemPriority } from '../../core/models/enums';

export const TASK_PRIORITY_LABELS = {
  [TaskItemPriority.Low]: 'Low',
  [TaskItemPriority.Medium]: 'Medium',
  [TaskItemPriority.High]: 'High',
  [TaskItemPriority.Critical]: 'Critical',
} as const;

export const TASK_PRIORITY_COLORS = {
  [TaskItemPriority.Low]: '',
  [TaskItemPriority.Medium]: 'primary',
  [TaskItemPriority.High]: 'accent',
  [TaskItemPriority.Critical]: 'warn',
} as const;
