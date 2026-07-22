import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { UserLookupResponse } from '../models/user';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private api = inject(ApiService);

  list() {
    return this.api.get<UserLookupResponse[]>('/api/users/lookup');
  }
}
