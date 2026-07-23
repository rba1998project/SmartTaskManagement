import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from '../services/api.service';
import { AuthResponse } from '../models/auth';

// Session token persistence with in-memory cache + sessionStorage fallback.
//
// Pattern:
//  - Writes go to both the in-memory signal and sessionStorage.
//  - Reads prefer the in-memory signal (fast, avoids storage events), falling back to sessionStorage.
//  - Clear wipes both stores so a stale token cannot survive a logout.
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly ACCESS_KEY = 'access_token';
  private readonly REFRESH_KEY = 'refresh_token';
  private readonly EXPIRES_KEY = 'access_token_expires_at';

  private accessTokenMemory = signal<string | null>(null);

  setTokens(data: AuthResponse): void {
    this.accessTokenMemory.set(data.accessToken);
    try {
      sessionStorage.setItem(this.ACCESS_KEY, data.accessToken);
      sessionStorage.setItem(this.REFRESH_KEY, data.refreshToken);
      sessionStorage.setItem(this.EXPIRES_KEY, data.accessTokenExpiresAt);
    } catch {
      sessionStorage.clear();
      this.accessTokenMemory.set(data.accessToken);
    }
  }

  getAccessToken(): string | null {
    return this.accessTokenMemory() ?? sessionStorage.getItem(this.ACCESS_KEY);
  }

  getRefreshToken(): string | null {
    return sessionStorage.getItem(this.REFRESH_KEY);
  }

  getAccessTokenExpiresAt(): string | null {
    return sessionStorage.getItem(this.EXPIRES_KEY);
  }

  clear(): void {
    this.accessTokenMemory.set(null);
    sessionStorage.removeItem(this.ACCESS_KEY);
    sessionStorage.removeItem(this.REFRESH_KEY);
    sessionStorage.removeItem(this.EXPIRES_KEY);
  }

  hasValidAccessToken(): boolean {
    const token = this.getAccessToken();
    const expiresAt = this.getAccessTokenExpiresAt();
    if (!token || !expiresAt) return false;
    return new Date(expiresAt) > new Date();
  }
}
