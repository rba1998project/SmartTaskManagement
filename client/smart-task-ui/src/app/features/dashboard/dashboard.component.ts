import { Component, inject, signal, computed, DestroyRef, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardService } from '../../core/services/dashboard.service';
import { ProjectsService } from '../../core/services/projects.service';
import { TasksService } from '../../core/services/tasks.service';
import { DashboardResponse } from '../../core/models/dashboard';
import { ProjectResponse } from '../../core/models/project';
import { TaskResponse } from '../../core/models/task';
import { TaskItemStatus, TaskItemPriority } from '../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS } from '../../shared/constants/task-priority.constants';

const STATUS_COLORS = ['#94A3B8', '#3B82F6', '#1F5D29', '#B82020'];
const PRIORITY_COLORS = ['#005C5E', '#EF5400', '#88114F', '#450000'];

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatListModule, MatProgressSpinnerModule, MatButtonModule, RouterModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private dashboardService = inject(DashboardService);
  private projectsService = inject(ProjectsService);
  private tasksService = inject(TasksService);
  private router = inject(Router);

  @ViewChild(BaseChartDirective) statusChart?: BaseChartDirective;
  @ViewChild(BaseChartDirective) priorityChart?: BaseChartDirective;

  readonly data = signal<DashboardResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly recentProjects = signal<ProjectResponse[]>([]);
  readonly recentTasks = signal<TaskResponse[]>([]);

  readonly statusLabels = signal<string[]>([]);
  readonly statusValues = signal<number[]>([]);
  readonly statusColors = signal<string[]>([]);

  readonly priorityLabels = signal<string[]>([]);
  readonly priorityValues = signal<number[]>([]);
  readonly priorityColors = signal<string[]>([]);

  statusPieOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 16,
          usePointStyle: true,
          pointStyleWidth: 10,
        },
      },
      datalabels: {
        color: '#ffffff',
        font: { weight: 'bold', size: 12 },
        padding: 6,
        formatter: (value: number) => value,
      },
    },
  };

  priorityPieOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 16,
          usePointStyle: true,
          pointStyleWidth: 10,
        },
      },
      datalabels: {
        color: '#ffffff',
        font: { weight: 'bold', size: 12 },
        padding: 6,
        formatter: (value: number) => value,
      },
    },
  };

  constructor() {
    this.load();
  }

  private syncCharts(d: DashboardResponse | null): void {
    if (!d) return;
    this.buildStatusChart(d);
    this.buildPriorityChart(d);
    Promise.resolve().then(() => {
      this.statusChart?.update();
      this.priorityChart?.update();
    });
  }

  private buildStatusChart(d: DashboardResponse): void {
    const labels: string[] = [];
    const values: number[] = [];
    const colors: string[] = [];
    const entries = Object.entries(d.tasksByStatus);
    entries.forEach(([key, value]) => {
      if (value <= 0) return;
      const numKey = Number(key);
      const label = TASK_STATUS_LABELS[numKey as TaskItemStatus] ?? String(key);
      labels.push(label);
      values.push(value);
      const colorIndex = numKey >= 0 && numKey < STATUS_COLORS.length ? numKey : labels.length - 1;
      colors.push(STATUS_COLORS[colorIndex] || '#B0BEC5');
    });
    this.statusLabels.set(labels);
    this.statusValues.set(values);
    this.statusColors.set(colors);
  }

  private buildPriorityChart(d: DashboardResponse): void {
    const labels: string[] = [];
    const values: number[] = [];
    const colors: string[] = [];
    const entries = Object.entries(d.tasksByPriority);
    entries.forEach(([key, value]) => {
      if (value <= 0) return;
      const numKey = Number(key);
      const label = TASK_PRIORITY_LABELS[numKey as TaskItemPriority] ?? String(key);
      labels.push(label);
      values.push(value);
      const colorIndex = numKey >= 0 && numKey < PRIORITY_COLORS.length ? numKey : labels.length - 1;
      colors.push(PRIORITY_COLORS[colorIndex] || '#B0BEC5');
    });
    this.priorityLabels.set(labels);
    this.priorityValues.set(values);
    this.priorityColors.set(colors);
  }

  readonly statusChartData = computed(() => ({
    labels: this.statusLabels(),
    datasets: [{ data: this.statusValues(), backgroundColor: this.statusColors() }]
  }));

  readonly priorityChartData = computed(() => ({
    labels: this.priorityLabels(),
    datasets: [{ data: this.priorityValues(), backgroundColor: this.priorityColors() }]
  }));

  statusChartClick(click: { active?: { index?: number }[] }): void {
    const index = click.active?.[0]?.index;
    const data = this.statusChartData();
    if (index == null || !data) return;
    const status = data.labels[index];
    if (status) {
      this.goToTasksByStatus(status);
    }
  }

  priorityChartClick(click: { active?: { index?: number }[] }): void {
    const index = click.active?.[0]?.index;
    const data = this.priorityChartData();
    if (index == null || !data) return;
    const priority = data.labels[index];
    if (priority) {
      this.goToTasksByPriority(priority);
    }
  }

  goToTasksByPriority(priority: string): void {
    this.router.navigate(['/tasks'], { queryParams: { priority } });
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.dashboardService.getStats().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          this.data.set(result.data);
          this.syncCharts(result.data);
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
      error: () => this.recentProjects.set([])
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

  goToUpcomingDue(): void {
    const today = new Date().toISOString().split('T')[0];
    this.router.navigate(['/tasks'], { queryParams: { dueDate: today } });
  }
}
