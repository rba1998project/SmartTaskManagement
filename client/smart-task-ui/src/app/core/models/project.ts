import { TaskItemStatus, TaskItemPriority, SortDirection } from './enums';

export interface ProjectResponse {
  id: string;
  name: string;
  description: string | null;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProjectQueryRequest {
  search?: string;
  sortField: ProjectSortField;
  sortDirection: SortDirection;
  pageNumber: number;
  pageSize: number;
}

export type ProjectSortField = 'Name' | 'CreatedAt' | 'UpdatedAt';
