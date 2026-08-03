import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskResponse, TaskStatusChangeResponse } from '../../../core/models/task';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { TASK_STATUS_LABELS, TASK_STATUS_COLORS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS, TASK_PRIORITY_COLORS } from '../../../shared/constants/task-priority.constants';

// Route: /tasks/:id
// Reads task id from route snapshot and loads the task detail.
@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatChipsModule,
    MatTableModule,
  ],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.css'
})
export class TaskDetailComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tasksService = inject(TasksService);
  private authService = inject(AuthService);

  readonly task = signal<TaskResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly statusHistory = signal<TaskStatusChangeResponse[]>([]);
  readonly historyLoading = signal(false);
  readonly historyError = signal<string | null>(null);
  readonly historyDisplayedColumns = ['fromStatus', 'toStatus', 'comment', 'changedBy', 'changedAt'];
  taskId = '';

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  canViewStatusHistory(): boolean {
    return this.canMutate();
  }

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id') || '';
    if (this.taskId) {
      this.loadTask();
    } else {
      this.loading.set(false);
      this.error.set('Task not found');
    }
  }

  loadTask(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tasksService.get(this.taskId).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.task.set(result.data);
          if (this.canViewStatusHistory()) {
            this.loadStatusHistory();
          }
        } else {
          this.error.set(result.message || 'Task not found');
        }
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.error.set(err.message || 'Failed to load task');
        this.loading.set(false);
      }
    });
  }

  loadStatusHistory(): void {
    this.historyLoading.set(true);
    this.historyError.set(null);

    this.tasksService.listStatusChanges(this.taskId).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.statusHistory.set(result.data);
        } else {
          this.historyError.set(result.message || 'Failed to load status history');
        }
        this.historyLoading.set(false);
      },
      error: (err: { message?: string }) => {
        this.historyError.set(err.message || 'Failed to load status history');
        this.historyLoading.set(false);
      },
    });
  }

  goToEdit(): void {
    this.router.navigate(['/tasks', this.taskId, 'edit']);
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
  }

  // Helpers for mat-chip color and label in the template
  statusLabel(status: TaskItemStatus): string {
    return TASK_STATUS_LABELS[status] ?? String(status);
  }

  priorityLabel(priority: TaskItemPriority): string {
    return TASK_PRIORITY_LABELS[priority] ?? String(priority);
  }

  statusColor(status: TaskItemStatus): string {
    return TASK_STATUS_COLORS[status] ?? '';
  }

  priorityColor(priority: TaskItemPriority): string {
    return TASK_PRIORITY_COLORS[priority] ?? '';
  }

  formatDate(date: string | null): string {
    if (!date) return 'No date set';
    return new Date(date).toLocaleDateString();
  }
}
