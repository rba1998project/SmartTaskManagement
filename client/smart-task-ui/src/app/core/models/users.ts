export interface UserListItem {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export type UserSortField = 'Name' | 'Email' | 'Role';

export interface UserQuery {
  search?: string;
  role?: string;
  sortField: UserSortField;
  sortDirection: 'Asc' | 'Desc';
  pageNumber: number;
  pageSize: number;
}

export interface UserPage {
  items: UserListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface UpdateUserRoleRequest {
  roleName: string;
}
