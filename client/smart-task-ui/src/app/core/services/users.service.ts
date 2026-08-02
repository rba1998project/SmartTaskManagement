import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { UserLookupResponse } from '../models/user';
import { PagedResult } from '../models/enums';
import { UserQuery, UserPage, UpdateUserRoleRequest } from '../models/users';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = inject(ApiService);

  list(params: { search?: string; pageNumber: number; pageSize: number }) {
    return this.api.get<PagedResult<UserLookupResponse>>('/api/users/assignees', {
      search: params.search,
      pageNumber: params.pageNumber,
      pageSize: params.pageSize,
    });
  }

  listUsers(params: UserQuery) {
    return this.api.get<UserPage>('/api/users', {
      search: params.search,
      role: params.role,
      sortField: params.sortField,
      sortDirection: params.sortDirection,
      pageNumber: params.pageNumber,
      pageSize: params.pageSize,
    });
  }

  updateRole(id: string, request: UpdateUserRoleRequest) {
    return this.api.put<ApiResponse<null>>(`/api/users/${id}/role`, request);
  }
}
