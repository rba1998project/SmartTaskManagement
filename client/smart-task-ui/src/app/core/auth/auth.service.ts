import { Injectable, inject, signal, computed } from '@angular/core';
import { ApiService } from '../services/api.service';
import { TokenStorageService } from './token-storage.service';
import { AuthResponse } from '../models/auth';
import { ApiResponse } from '../models/api-response';
import { UserRole } from '../models/enums';
import { Router } from '@angular/router';
import { catchError, map, Observable, of, throwError } from 'rxjs';

// Auth orchestration service.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = inject(ApiService);
  private tokenStorage = inject(TokenStorageService);
  private router = inject(Router);

  private _currentUser = signal<AuthResponse | null>(null);
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());

  get accessToken(): string | null {
    return this.tokenStorage.getAccessToken();
  }

  get refreshTokenValue(): string | null {
    return this.tokenStorage.getRefreshToken();
  }

  get accessTokenExpiresAt(): string | null {
    return this.tokenStorage.getAccessTokenExpiresAt();
  }

  login(request: { email: string; password: string }): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/login', request).pipe(
      map((response: ApiResponse<AuthResponse>) => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Login failed');
        }
        this.tokenStorage.setTokens(response.data);
        this._currentUser.set(response.data);
        return response.data;
      }),
      catchError(err => throwError(() => err))
    );
  }

  register(request: { email: string; password: string; fullName: string }): Observable<AuthResponse> {
    return this.api.post<AuthResponse>('/api/auth/register', request).pipe(
      map((response: ApiResponse<AuthResponse>) => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Registration failed');
        }
        return response.data;
      }),
      catchError(err => throwError(() => err))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token'));
    }

    return this.api.post<AuthResponse>('/api/auth/refresh', { refreshToken }).pipe(
      map((response: ApiResponse<AuthResponse>) => {
        if (!response.success || !response.data) {
          this.logout();
          throw new Error(response.message || 'Refresh failed');
        }
        this.tokenStorage.setTokens(response.data);
        this._currentUser.set(response.data);
        return response.data;
      }),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  logout(): void {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (refreshToken) {
      this.api.post<ApiResponse<null>>('/api/auth/logout', { refreshToken }).subscribe({
        error: () => {}
      });
    }
    this.tokenStorage.clear();
    this._currentUser.set(null);
  }

  autoLogin(): Observable<boolean> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return of(false);
    }

    return this.refreshToken().pipe(
      map(() => true),
      catchError(() => {
        this.logout();
        return of(false);
      })
    );
  }

  hasRole(role: UserRole): boolean {
    const roles = this._currentUser()?.roles ?? [];
    return roles.includes(String(role));
  }

  hasAnyRole(roles: UserRole[]): boolean {
    if (!roles || roles.length === 0) return true;
    return roles.some(role => this.hasRole(role));
  }
}
