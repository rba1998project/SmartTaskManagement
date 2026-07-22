import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response';

// Shared query params for list endpoints.
export interface ListParams {
  search?: string;
  sortField?: string;
  sortDirection?: string;
  pageNumber: number;
  pageSize: number;
}

// Generic HTTP wrapper that prefixes the API base URL and wraps responses in ApiResponse<T>.
// All feature services should use this instead of injecting HttpClient directly.
@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl;

  get<T>(path: string, params?: Record<string, string | number | boolean | undefined>): import('rxjs').Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(`${this.baseUrl}${path}`, { params: this.buildParams(params) });
  }

  post<T>(path: string, body: unknown): import('rxjs').Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(`${this.baseUrl}${path}`, body);
  }

  put<T>(path: string, body: unknown): import('rxjs').Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(`${this.baseUrl}${path}`, body);
  }

  delete<T>(path: string): import('rxjs').Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(`${this.baseUrl}${path}`);
  }

  private buildParams(input?: Record<string, string | number | boolean | undefined>): Record<string, string | string[]> {
    if (!input) return {};
    const out: Record<string, string | string[]> = {};
    for (const key of Object.keys(input)) {
      const value = input[key];
      if (value !== undefined && value !== null && value !== '') {
        out[key] = String(value);
      }
    }
    return out;
  }
}
