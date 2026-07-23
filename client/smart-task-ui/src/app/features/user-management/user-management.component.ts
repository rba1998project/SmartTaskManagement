import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { UsersService } from '../../core/services/users.service';
import { NotificationService } from '../../core/services/notification.service';
import { UserManagementResponse, UpdateUserRoleRequest } from '../../core/models/user-management';
import { UserRole } from '../../core/models/enums';

// Route: /users
// Admin-only user management page.
@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    RouterModule,
  ],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private usersService = inject(UsersService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  readonly users = signal<UserManagementResponse[]>([]);
  readonly loading = signal(false);
  readonly saving = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  readonly roleOptions = [
    { value: UserRole.Admin, label: 'Admin' },
    { value: UserRole.ProjectManager, label: 'Project Manager' },
    { value: UserRole.TeamMember, label: 'Team Member' },
  ];

  readonly displayedColumns = ['email', 'fullName', 'role', 'actions'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.usersService.listAll().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success && result.data) {
          this.users.set(result.data);
        } else {
          this.error.set(result.message || 'Failed to load users');
        }
      },
      error: (err: { message?: string }) => {
        this.notificationService.showError(err.message || 'Failed to load users');
        this.error.set(err.message || 'Failed to load users');
        this.loading.set(false);
      }
    });
  }

  saveRole(user: UserManagementResponse): void {
    if (this.saving()) return;
    this.saving.set(user.id);
    const request: UpdateUserRoleRequest = { roleName: user.role };
    this.usersService.updateRole(user.id, request).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.saving.set(null);
        if (result.success) {
          this.notificationService.showSuccess(`Role updated for ${user.email}`);
        } else {
          this.notificationService.showError(result.message || 'Failed to update role');
        }
      },
      error: (err: { message?: string }) => {
        this.notificationService.showError(err.message || 'Failed to update role');
        this.saving.set(null);
      }
    });
  }

  onRoleChange(user: UserManagementResponse, role: string): void {
    user.role = role;
  }

  canManage(): boolean {
    return this.router.url.startsWith('/users');
  }
}