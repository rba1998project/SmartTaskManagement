import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { ProjectResponse, ProjectQueryRequest } from '../models/project';
import { PagedResult } from '../models/enums';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private api = inject(ApiService);

  list(params: ProjectQueryRequest) {
    return this.api.get<PagedResult<ProjectResponse>>('/api/projects', {
      search: params.search,
      sortField: params.sortField,
      sortDirection: params.sortDirection,
      pageNumber: params.pageNumber,
      pageSize: params.pageSize,
    });
  }

  get(id: string) {
    return this.api.get<ProjectResponse>(`/api/projects/${id}`);
  }

  create(data: { name: string; description?: string }) {
    return this.api.post<ProjectResponse>('/api/projects', data);
  }

  update(id: string, data: { name: string; description?: string }) {
    return this.api.put<ProjectResponse>(`/api/projects/${id}`, data);
  }

  delete(id: string) {
    return this.api.delete<ProjectResponse>(`/api/projects/${id}`);
  }

  recent() {
    return this.api.get<PagedResult<ProjectResponse>>('/api/projects', {
      sortField: 'CreatedAt',
      sortDirection: 'Desc',
      pageNumber: 1,
      pageSize: 5,
    });
  }
}
