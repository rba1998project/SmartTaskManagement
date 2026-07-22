export enum UserRole {
  Admin = 1,
  ProjectManager = 2,
  TeamMember = 3
}

export enum TaskItemStatus {
  ToDo = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3
}

export enum TaskItemPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export const SortDirection = {
  Asc: 'Asc',
  Desc: 'Desc',
} as const;
export type SortDirection = typeof SortDirection[keyof typeof SortDirection];

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
