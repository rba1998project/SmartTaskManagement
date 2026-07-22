import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { DashboardResponse } from '../models/dashboard';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private api = inject(ApiService);

  getStats() {
    return this.api.get<DashboardResponse>('/api/dashboard');
  }
}
