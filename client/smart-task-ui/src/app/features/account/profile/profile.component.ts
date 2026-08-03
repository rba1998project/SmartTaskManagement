import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { ChangePasswordDialogComponent } from '../change-password/change-password-dialog.component';

// Route: /account
// Displays the current user's email, full name, and roles.
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);

  readonly currentUser = computed(() => this.authService.currentUser());

  openChangePassword(): void {
    this.dialog.open(ChangePasswordDialogComponent, {
      width: '480px',
      maxWidth: 'calc(100vw - 32px)',
    });
  }

  // Used for conditional account sections.
  isAdmin(): boolean {
    return this.authService.hasRole(UserRole.Admin);
  }
}
