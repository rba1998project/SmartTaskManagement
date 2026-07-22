import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response';

export interface AiStatus {
  enabled: boolean;
  provider?: string;
  model?: string;
}

// AI toggle state provider.
@Injectable({ providedIn: 'root' })
export class AiAvailabilityService {
  private api = inject(ApiService);
  readonly enabled = signal<boolean>(false);
  readonly status = signal<AiStatus | null>(null);

  checkStatus(): void {
    this.api.get<AiStatus>('/api/ai/status').subscribe({
      next: (response: ApiResponse<AiStatus>) => {
        if (response.success && response.data) {
          this.enabled.set(response.data.enabled);
          this.status.set(response.data);
        }
      },
      error: () => {
        this.enabled.set(false);
        this.status.set(null);
      }
    });
  }
}
