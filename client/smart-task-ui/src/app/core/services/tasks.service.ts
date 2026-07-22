import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { TaskResponse, TaskQueryRequest } from '../models/task';
import { PagedResult } from '../models/enums';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private api = inject(ApiService);

  list(params: TaskQueryRequest) {
    return this.api.get<PagedResult<TaskResponse>>('/api/tasks', {
      search: params.search,
      status: params.status,
      priority: params.priority,
      assignedToUserId: params.assignedToUserId,
      dueDate: params.dueDate,
      sortField: params.sortField,
      sortDirection: params.sortDirection,
      pageNumber: params.pageNumber,
      pageSize: params.pageSize,
    });
  }

  get(id: string) {
    return this.api.get<TaskResponse>(`/api/tasks/${id}`);
  }

  create(projectId: string, data: { title: string; description?: string; status: number; priority: number; dueDate?: string }) {
    return this.api.post<TaskResponse>(`/api/projects/${projectId}/tasks`, data);
  }

  update(id: string, data: { title: string; description?: string; status: number; priority: number; dueDate?: string }) {
    return this.api.put<TaskResponse>(`/api/tasks/${id}`, data);
  }

  delete(id: string) {
    return this.api.delete<TaskResponse>(`/api/tasks/${id}`);
  }

  updateStatus(id: string, status: number) {
    return this.api.put<TaskResponse>(`/api/tasks/${id}/status`, { status });
  }

  assign(id: string, userId?: string) {
    return this.api.put<TaskResponse>(`/api/tasks/${id}/assignment`, { assignedToUserId: userId });
  }

  recent() {
    return this.api.get<PagedResult<TaskResponse>>('/api/tasks', {
      sortField: 'CreatedAt',
      sortDirection: 'Desc',
      pageNumber: 1,
      pageSize: 5,
    });
  }

  listByProject(projectId: string) {
    return this.api.get<TaskResponse[]>(`/api/projects/${projectId}/tasks`);
  }

  improveDescription(request: { description: string }) {
    return this.api.post<{ improvedDescription: string }>('/api/tasks/improve-description', request);
  }
}
