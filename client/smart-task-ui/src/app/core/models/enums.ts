export enum UserRole {
  Admin = 'Admin',
  ProjectManager = 'ProjectManager',
  TeamMember = 'TeamMember'
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

// NOTE: These enums use numeric values. The backend API contract expects these exact numeric
// values in both directions (requests and responses). Do NOT change the numbers or switch to
// string enums without also updating the backend serialization and EF Core configuration.

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
