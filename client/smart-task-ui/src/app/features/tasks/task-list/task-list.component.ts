import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskResponse, TaskQueryRequest } from '../../../core/models/task';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS } from '../../../shared/constants/task-priority.constants';

// Route: /tasks
// Loads paginated, sortable, and filterable task list.
@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatOptionModule,
    MatChipsModule,
    RouterModule,
  ],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent implements OnInit {
  private tasksService = inject(TasksService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  readonly tasks = signal<TaskResponse[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly search = signal('');
  readonly status = signal<TaskItemStatus | undefined>(undefined);
  readonly priority = signal<TaskItemPriority | undefined>(undefined);
  readonly sortField = signal<TaskQueryRequest['sortField']>('CreatedAt');
  readonly sortDirection = signal<'Asc' | 'Desc'>('Desc');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(10);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  displayedColumns: string[] = ['title', 'projectId', 'status', 'priority', 'dueDate', 'assignedToUserId', 'actions'];

  currentUserId = this.authService.currentUser()?.userId ?? '';

  readonly statusOptions = [
    { value: undefined as TaskItemStatus | undefined, label: 'All' },
    { value: TaskItemStatus.ToDo, label: TASK_STATUS_LABELS[TaskItemStatus.ToDo] },
    { value: TaskItemStatus.InProgress, label: TASK_STATUS_LABELS[TaskItemStatus.InProgress] },
    { value: TaskItemStatus.Completed, label: TASK_STATUS_LABELS[TaskItemStatus.Completed] },
    { value: TaskItemStatus.Cancelled, label: TASK_STATUS_LABELS[TaskItemStatus.Cancelled] },
  ];

  readonly priorityOptions = [
    { value: undefined as TaskItemPriority | undefined, label: 'All' },
    { value: TaskItemPriority.Low, label: TASK_PRIORITY_LABELS[TaskItemPriority.Low] },
    { value: TaskItemPriority.Medium, label: TASK_PRIORITY_LABELS[TaskItemPriority.Medium] },
    { value: TaskItemPriority.High, label: TASK_PRIORITY_LABELS[TaskItemPriority.High] },
    { value: TaskItemPriority.Critical, label: TASK_PRIORITY_LABELS[TaskItemPriority.Critical] },
  ];

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    const params: TaskQueryRequest = {
      search: this.search() || undefined,
      status: this.status(),
      priority: this.priority(),
      sortField: this.sortField(),
      sortDirection: this.sortDirection(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    };

    this.tasksService.list(params).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.tasks.set(result.data.items);
          this.totalCount.set(result.data.totalCount);
          this.totalPages.set(result.data.totalPages);
        } else {
          this.error.set(result.message || 'Failed to load tasks');
        }
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.error.set(err.message || 'Network error');
        this.loading.set(false);
      }
    });
  }

  onSearch(value: string): void {
    this.search.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  // Reset filters and reload
  onStatusChange(value: TaskItemStatus | undefined): void {
    this.status.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  // Reset filters and reload
  onPriorityChange(value: TaskItemPriority | undefined): void {
    this.priority.set(value);
    this.pageNumber.set(1);
    this.load();
  }

  // Map matSort event to service query params
  onSort(sort: { active: string; direction: string }): void {
    if (!sort.active || !sort.direction) return;
    this.sortField.set(sort.active as TaskQueryRequest['sortField']);
    this.sortDirection.set(sort.direction as 'Asc' | 'Desc');
    this.pageNumber.set(1);
    this.load();
  }

  // Sync mat-paginator to reactive signals
  onPageChange(event: { pageIndex: number; pageSize: number }): void {
    this.pageNumber.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.load();
  }

  goToCreate(): void {
    this.router.navigate(['/tasks/create']);
  }

  goToEdit(id: string): void {
    this.router.navigate(['/tasks', id, 'edit']);
  }

  goToDetail(id: string): void {
    this.router.navigate(['/tasks', id]);
  }

  // Delete task via confirmation dialog
  deleteTask(task: TaskResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete "${task.title}"?`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.tasksService.delete(task.id).subscribe({
          next: () => {
            this.load();
          },
          error: (err: { message?: string }) => {
            this.error.set(err.message || 'Failed to delete task');
          }
        });
      }
    });
  }

  // Improve mat-table rendering performance
  trackByTaskId(index: number, task: TaskResponse): string {
    return task.id;
  }

  statusLabel(status: TaskItemStatus): string {
    return TASK_STATUS_LABELS[status] ?? String(status);
  }

  priorityLabel(priority: TaskItemPriority): string {
    return TASK_PRIORITY_LABELS[priority] ?? String(priority);
  }

  statusColor(status: TaskItemStatus): string {
    const colors: Record<TaskItemStatus, string> = {
      [TaskItemStatus.ToDo]: 'primary',
      [TaskItemStatus.InProgress]: 'accent',
      [TaskItemStatus.Completed]: '',
      [TaskItemStatus.Cancelled]: 'warn',
    };
    return colors[status] ?? '';
  }

  priorityColor(priority: TaskItemPriority): string {
    const colors: Record<TaskItemPriority, string> = {
      [TaskItemPriority.Low]: '',
      [TaskItemPriority.Medium]: 'primary',
      [TaskItemPriority.High]: 'accent',
      [TaskItemPriority.Critical]: 'warn',
    };
    return colors[priority] ?? '';
  }
}
