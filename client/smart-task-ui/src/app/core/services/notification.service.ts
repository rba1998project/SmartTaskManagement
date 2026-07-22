import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
// Toast notification wrapper.
export class NotificationService {
  private snackBar = inject(MatSnackBar);

  show(message: string, duration: number = 3000, config: MatSnackBarConfig = {}): void {
    const defaults: MatSnackBarConfig = {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      ...config,
    };
    this.snackBar.open(message, 'Close', defaults);
  }

  showSuccess(message: string): void {
    this.show(message, 3000, { panelClass: 'snackbar-success' });
  }

  showError(message: string): void {
    this.show(message, 5000, { panelClass: 'snackbar-error' });
  }
}
