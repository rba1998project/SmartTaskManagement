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
import { ProjectsService } from '../../../core/services/projects.service';
import { TasksService } from '../../../core/services/tasks.service';
import { ProjectResponse } from '../../../core/models/project';
import { TaskResponse } from '../../../core/models/task';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { AuthService } from '../../../core/auth/auth.service';
import { UserRole } from '../../../core/models/enums';
import { TASK_STATUS_LABELS, TASK_STATUS_COLORS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS, TASK_PRIORITY_COLORS } from '../../../shared/constants/task-priority.constants';

// Route: /projects/:id
// Reads the project id from the route snapshot and loads the project.
@Component({
  selector: 'app-project-detail',
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
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.css'
})
export class ProjectDetailComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private projectsService = inject(ProjectsService);
  private tasksService = inject(TasksService);
  private authService = inject(AuthService);

  readonly project = signal<ProjectResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly tasks = signal<TaskResponse[]>([]);
  readonly tasksLoading = signal(false);
  readonly tasksError = signal<string | null>(null);
  projectId = '';

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') || '';
    if (this.projectId) {
      this.loadProject();
    } else {
      this.loading.set(false);
      this.error.set('Project not found');
    }
  }

  loadProject(): void {
    this.loading.set(true);
    this.error.set(null);

    this.projectsService.get(this.projectId).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.loading.set(false);
        if (result.success && result.data) {
          this.project.set(result.data);
          this.loadTasks();
        } else {
          this.error.set(result.message || 'Project not found');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.message || 'Failed to load project');
      }
    });
  }

  loadTasks(): void {
    this.tasksLoading.set(true);
    this.tasksError.set(null);

    this.tasksService.listByProject(this.projectId).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.tasksLoading.set(false);
        if (result.success && result.data) {
          this.tasks.set(result.data);
        } else {
          this.tasks.set([]);
          this.tasksError.set(result.message || 'Failed to load tasks');
        }
      },
      error: (err) => {
        this.tasksLoading.set(false);
        this.tasks.set([]);
        this.tasksError.set(err.message || 'Failed to load tasks');
      }
    });
  }

  // Navigate to the project edit route: /projects/:id/edit
  goToEdit(): void {
    this.router.navigate(['/projects', this.projectId, 'edit']);
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }

  statusLabel(status: TaskItemStatus): string {
    return TASK_STATUS_LABELS[status] ?? String(status);
  }

  statusColor(status: TaskItemStatus): string {
    return TASK_STATUS_COLORS[status] ?? '';
  }

  priorityLabel(priority: TaskItemPriority): string {
    return TASK_PRIORITY_LABELS[priority] ?? String(priority);
  }

  priorityColor(priority: TaskItemPriority): string {
    return TASK_PRIORITY_COLORS[priority] ?? '';
  }

  formatDate(date: string | null): string {
    if (!date) return 'No date set';
    return new Date(date).toLocaleDateString();
  }
}
