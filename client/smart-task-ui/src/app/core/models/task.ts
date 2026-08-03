import { TaskItemStatus, TaskItemPriority, SortDirection } from './enums';

export interface TaskResponse {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  priority: TaskItemPriority;
  dueDate: string | null;
  assignedToUserId: string | null;
  assignedToUserName: string | null;
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

export interface UpdateTaskStatusRequest {
  status: TaskItemStatus;
  comment: string;
}

export interface TaskStatusChangeResponse {
  id: string;
  fromStatus: TaskItemStatus;
  toStatus: TaskItemStatus;
  comment: string | null;
  changedByUserId: string;
  changedByDisplayName: string;
  changedAt: string;
}
