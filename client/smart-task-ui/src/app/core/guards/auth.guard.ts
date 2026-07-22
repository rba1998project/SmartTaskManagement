import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

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
