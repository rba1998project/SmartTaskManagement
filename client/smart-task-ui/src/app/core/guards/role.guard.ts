import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';
import { UserRole } from '../models/enums';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const requiredRoles = route.data['roles'] as UserRole[];

  if (requiredRoles && !authService.hasAnyRole(requiredRoles)) {
    return router.createUrlTree(['/403']);
  }

  return true;
};
