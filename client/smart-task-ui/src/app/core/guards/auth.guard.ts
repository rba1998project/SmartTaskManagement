import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

// Allows only authenticated users; attempts silent refresh when possible.
// flow:
//   1. If already authenticated -> allow.
//   2. If not authenticated but refresh token exists -> attempt silent refresh.
//   3. If refresh succeeds -> allow; if refresh fails -> clear state and redirect to /login.
//   4. If no refresh token -> redirect to /login.
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  const refreshToken = authService.refreshTokenValue;
  if (refreshToken) {
    return authService.refreshToken().pipe(
      map(() => true),
      catchError(() => {
        authService.logout();
        return of(router.createUrlTree(['/login']));
      })
    );
  }

  return router.createUrlTree(['/login']);
};
