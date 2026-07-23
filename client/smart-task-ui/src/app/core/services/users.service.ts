import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { UserLookupResponse } from '../models/user';
import { UserManagementResponse, UpdateUserRoleRequest } from '../models/user-management';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = inject(ApiService);

  list() {
    return this.api.get<UserLookupResponse[]>('/api/users/assignees');
  }

  listAll() {
    return this.api.get<UserManagementResponse[]>('/api/users');
  }

  updateRole(id: string, request: UpdateUserRoleRequest) {
    return this.api.put<ApiResponse<null>>(`/api/users/${id}/role`, request);
  }
}
