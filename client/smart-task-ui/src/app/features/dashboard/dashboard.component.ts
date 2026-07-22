import { Component, inject, signal, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { ProjectsService } from '../../core/services/projects.service';
import { TasksService } from '../../core/services/tasks.service';
import { DashboardResponse } from '../../core/models/dashboard';
import { ProjectResponse } from '../../core/models/project';
import { TaskResponse } from '../../core/models/task';
import { TaskItemStatus } from '../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../shared/constants/task-status.constants';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatListModule, MatProgressSpinnerModule, MatButtonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private dashboardService = inject(DashboardService);
  private projectsService = inject(ProjectsService);
  private tasksService = inject(TasksService);
  private router = inject(Router);

  readonly data = signal<DashboardResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly recentProjects = signal<ProjectResponse[]>([]);
  readonly recentTasks = signal<TaskResponse[]>([]);
  readonly recentLoading = signal(true);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getStats().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.data.set(result.data);
        } else {
          this.error.set(result.message || 'Failed to load dashboard');
        }
        this.loading.set(false);
      },
      error: (err: Error) => {
        this.error.set(err.message || 'Network error');
        this.loading.set(false);
      }
    });

    this.projectsService.recent().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.recentProjects.set(result.data.items);
        } else {
          this.recentProjects.set([]);
        }
      },
      error: () => this.recentProjects.set([]),
      complete: () => this.recentLoading.set(false)
    });

    this.tasksService.recent().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.recentTasks.set(result.data.items);
        } else {
          this.recentTasks.set([]);
        }
      },
      error: () => this.recentTasks.set([])
    });
  }

  getStatusValue(record: Record<number, number> | undefined, status: number): number {
    return record?.[status] ?? 0;
  }

  taskStatusLabel(status: number): string {
    return TASK_STATUS_LABELS[status as TaskItemStatus] ?? String(status);
  }

  goToProjects(): void {
    this.router.navigate(['/projects']);
  }

  goToTasks(): void {
    this.router.navigate(['/tasks']);
  }

  goToTasksByStatus(status: string): void {
    this.router.navigate(['/tasks'], { queryParams: { status } });
  }

  goToPending(): void {
    this.router.navigate(['/tasks']);
  }

  goToUpcomingDue(): void {
    const today = new Date().toISOString().split('T')[0];
    this.router.navigate(['/tasks'], { queryParams: { dueDate: today } });
  }
}
