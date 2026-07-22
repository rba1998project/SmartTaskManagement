import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskResponse } from '../../../core/models/task';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS } from '../../../shared/constants/task-priority.constants';

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
  ],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.css'
})
export class TaskDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tasksService = inject(TasksService);
  private authService = inject(AuthService);

  readonly task = signal<TaskResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  taskId = '';

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id') || '';
    if (this.taskId) {
      this.loadTask();
    }
  }

  loadTask(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tasksService.get(this.taskId).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.task.set(result.data);
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

  goToEdit(): void {
    this.router.navigate(['/tasks', this.taskId, 'edit']);
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
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

  formatDate(date: string | null): string {
    if (!date) return 'No date set';
    return new Date(date).toLocaleDateString();
  }
}