import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction, Subject } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, distinctUntilChanged, filter } from 'rxjs/operators';
import { UsersService } from '../../core/services/users.service';
import { NotificationService } from '../../core/services/notification.service';
import { UserQuery, UserListItem, UpdateUserRoleRequest, UserSortField } from '../../core/models/users';
import { UserRole } from '../../core/models/enums';
import { AuthService } from '../../core/auth/auth.service';

// Route: /users
// Admin-only user management page.
@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatInputModule,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private usersService = inject(UsersService);
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  readonly users = signal<UserListItem[]>([]);
  readonly loading = signal(false);
  readonly saving = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly search = signal('');
  readonly roleFilter = signal<string | undefined>(undefined);
  readonly sortField = signal<UserSortField>('Email');
  readonly sortDirection = signal<'Asc' | 'Desc'>('Asc');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);
  private readonly searchChanges = new Subject<{ value: string; version: number }>();
  private searchVersion = 0;

  readonly roleOptions = [
    { value: UserRole.Admin, label: 'Admin' },
    { value: UserRole.ProjectManager, label: 'Project Manager' },
    { value: UserRole.TeamMember, label: 'Team Member' },
  ];

  readonly displayedColumns = ['email', 'fullName', 'role', 'actions'];

  ngOnInit(): void {
    this.searchChanges.pipe(
      debounceTime(300),
      filter(({ version }) => version === this.searchVersion),
      distinctUntilChanged((previous, current) => previous.value === current.value),
      this.untilDestroyed
    ).subscribe(() => {
      this.pageNumber.set(1);
      this.load();
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    const query: UserQuery = {
      search: this.search().trim() || undefined,
      role: this.roleFilter(),
      sortField: this.sortField(),
      sortDirection: this.sortDirection(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    };
    this.usersService.listUsers(query).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success && result.data) {
          this.users.set(result.data.items);
          this.totalCount.set(result.data.totalCount);
          this.totalPages.set(result.data.totalPages);
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

  onSearch(term: string): void {
    this.search.set(term);
    this.searchVersion++;
    this.searchChanges.next({ value: term, version: this.searchVersion });
  }

  onRoleFilterChange(role: string): void {
    this.roleFilter.set(role || undefined);
    this.pageNumber.set(1);
    this.load();
  }

  onSort(sort: { active: string; direction: string }): void {
    if (!sort.active || !sort.direction) return;
    this.sortField.set(sort.active as UserSortField);
    this.sortDirection.set(sort.direction as 'Asc' | 'Desc');
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  resetFilters(): void {
    const isAlreadyReset =
      this.search() === '' &&
      this.roleFilter() === undefined &&
      this.pageNumber() === 1;

    if (isAlreadyReset) return;

    this.searchVersion++;
    this.roleFilter.set(undefined);
    this.pageNumber.set(1);
    this.search.set('');
    this.load();
  }

  saveRole(user: UserListItem): void {
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

  onRoleChange(user: UserListItem, role: string): void {
    user.role = role;
  }

  isCurrentUser(user: UserListItem): boolean {
    return user.id === this.authService.currentUser()?.userId;
  }

}
