import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';

// Route: /account
// Displays the current user's email, full name, and roles.
// Change Password is hidden/placeholder because the backend has no change-password endpoint.
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, MatIconModule, MatTooltipModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  private authService = inject(AuthService);

  readonly currentUser = computed(() => this.authService.currentUser());

  // Used for conditional sections (e.g., admin-only placeholders).
  isAdmin(): boolean {
    return this.authService.hasRole(UserRole.Admin);
  }
}
