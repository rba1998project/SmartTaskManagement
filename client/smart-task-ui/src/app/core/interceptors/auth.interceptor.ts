import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { filter, take, concatMap, catchError, tap, map } from 'rxjs/operators';
import { AuthResponse } from '../models/auth';

let isRefreshing = false;
let refreshTokenSubject = new BehaviorSubject<string | null>(null);

const excludePaths = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/ai/status'];

// Attaches Bearer tokens to outgoing requests and handles silent refresh on expiry.
//
// Behavior:
//  1. Skip auth for public auth endpoints (login/register/refresh/ai-status).
//  2. Attach access token if available.
//  3. If token is near expiry, queue request and refresh first.
//  4. If request returns 401, attempt refresh once more then logout + redirect to /login.
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (excludePaths.some(path => req.urlWithParams.includes(path) || req.url.includes(path))) {
    return next(req);
  }

  const token = authService.accessToken;
  let authReq = req;

  if (token) {
    authReq = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  if (token && authService.accessTokenExpiresAt) {
    const expiresAt = new Date(authService.accessTokenExpiresAt);
    if (expiresAt <= new Date(Date.now() + 2 * 60 * 1000)) {
      return handleRefresh(authService, router).pipe(
        concatMap((newToken) => {
          return next(authReq.clone({
            setHeaders: { Authorization: `Bearer ${newToken}` }
          }));
        }),
        catchError((err) => {
          return throwError(() => err);
        })
      );
    }
  }

  return next(authReq).pipe(
    catchError((error) => {
      if (error.status === 401) {
        return handleRefresh(authService, router).pipe(
          concatMap((newToken) => {
            return next(authReq.clone({
              setHeaders: { Authorization: `Bearer ${newToken}` }
            }));
          }),
          catchError((err) => {
            authService.logout();
            router.navigate(['/login']);
            return throwError(() => err);
          })
        );
      }
      return throwError(() => error);
    })
  );
};

function handleRefresh(authService: AuthService, router: Router): Observable<string> {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    return authService.refreshToken().pipe(
      tap((response: AuthResponse) => {
        isRefreshing = false;
        refreshTokenSubject.next(response.accessToken);
      }),
      map((response: AuthResponse) => response.accessToken),
      catchError((err) => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        authService.logout();
        router.navigate(['/login']);
        return throwError(() => err);
      })
    );
  }

  return refreshTokenSubject.pipe(
    filter((token): token is string => token != null),
    take(1)
  );
}
