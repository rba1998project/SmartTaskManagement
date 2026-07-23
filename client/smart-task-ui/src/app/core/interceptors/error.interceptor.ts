import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

const excludePaths = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh', '/api/ai/status'];

function sanitizeErrorMessage(message: string | undefined): string {
  if (!message) return 'An unexpected error occurred.';
  return message.replace(/https?:\/\/[^\s]+/g, '[hidden]').trim() || 'An unexpected error occurred.';
}

// Maps HTTP errors to user-friendly toast messages.
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notificationService = inject(NotificationService);

  const isAuthEndpoint = excludePaths.some(path => req.urlWithParams.includes(path) || req.url.includes(path));

  return next(req).pipe(
    catchError((error) => {
      if (error.status === 0) {
        notificationService.showError('Network error. Please check your connection.');
      } else if (error.status === 400) {
        const errors = error.error?.errors || [error.error?.message];
        if (errors && errors.length > 0) {
          notificationService.showError(errors.join(', '));
        } else {
          notificationService.showError('Invalid request. Please check your input.');
        }
      } else if (error.status === 401 && !isAuthEndpoint) {
        notificationService.showError('Session expired. Please log in again.');
      } else if (error.status === 403) {
        notificationService.showError('You do not have permission to perform this action.');
        if (!req.urlWithParams.includes('/403')) {
          router.navigate(['/403']);
        }
      } else if (error.status === 404) {
        notificationService.showError('The requested resource was not found.');
      } else if (error.status === 429) {
        notificationService.showError('Too many requests. Please wait a moment.');
      } else if (error.status >= 500) {
        notificationService.showError('An unexpected error occurred.');
      } else {
        notificationService.showError('An unexpected error occurred.');
      }

      const apiMessage = isAuthEndpoint
        ? (error.error?.errors?.[0] || error.error?.message || error.message)
        : error.message;

      const sanitized = { ...error, message: sanitizeErrorMessage(apiMessage) };
      return throwError(() => sanitized);
    })
  );
};
