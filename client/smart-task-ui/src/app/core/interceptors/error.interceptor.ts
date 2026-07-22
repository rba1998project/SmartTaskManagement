import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

function sanitizeErrorMessage(message: string | undefined): string {
  if (!message) return 'An unexpected error occurred.';
  return message.replace(/https?:\/\/[^\s]+/g, '[hidden]').trim() || 'An unexpected error occurred.';
}

// Maps HTTP errors to user-friendly toast messages.
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const notificationService = inject(NotificationService);

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
      } else if (error.status === 401) {
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

      const sanitized = { ...error, message: sanitizeErrorMessage(error.message) };
      return throwError(() => sanitized);
    })
  );
};
