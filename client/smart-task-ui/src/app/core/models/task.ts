import { TaskItemStatus, TaskItemPriority, SortDirection } from './enums';

export interface TaskResponse {
  id: string;
  projectId: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  priority: TaskItemPriority;
  dueDate: string | null;
  assignedToUserId: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TaskQueryRequest {
  search?: string;
  status?: TaskItemStatus;
  priority?: TaskItemPriority;
  assignedToUserId?: string;
  dueDate?: string;
  sortField: TaskSortField;
  sortDirection: SortDirection;
  pageNumber: number;
  pageSize: number;
}

export type TaskSortField = 'Title' | 'CreatedAt' | 'DueDate' | 'Priority' | 'Status';

export interface ImproveDescriptionRequest {
  description: string;
}

export interface ImproveDescriptionResponse {
  improvedDescription: string;
}
