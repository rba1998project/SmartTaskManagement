import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TasksService } from '../../../core/services/tasks.service';
import { ProjectsService } from '../../../core/services/projects.service';
import { UsersService } from '../../../core/services/users.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ProjectResponse } from '../../../core/models/project';
import { UserLookupResponse } from '../../../core/models/user';
import { UserRole } from '../../../core/models/enums';
import { AuthService } from '../../../core/auth/auth.service';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS } from '../../../shared/constants/task-priority.constants';
import { AiEnhanceButtonComponent } from '../../../shared/components/ai-enhance-button/ai-enhance-button.component';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatSelectModule,
    MatOptionModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatAutocompleteModule,
    AiEnhanceButtonComponent,
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css'
})
export class TaskFormComponent implements OnInit {
  private untilDestroyed: OperatorFunction<any, any> = takeUntilDestroyed(inject(DestroyRef));
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tasksService = inject(TasksService);
  private projectsService = inject(ProjectsService);
  private usersService = inject(UsersService);
  private notificationService = inject(NotificationService);
  private authService = inject(AuthService);

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    status: [TaskItemStatus.ToDo, Validators.required],
    priority: [TaskItemPriority.Low, Validators.required],
    dueDate: [null as string | null],
    projectId: ['', Validators.required],
    assignedToUserId: [''],
  });

  loading = signal(false);
  isEdit = false;
  taskId: string | null = null;
  originalAssignedToUserId: string | null | undefined;

  readonly projects = signal<ProjectResponse[]>([]);
  readonly projectsLoading = signal(false);
  readonly filteredProjects = signal<ProjectResponse[]>([]);
  readonly users = signal<UserLookupResponse[]>([]);
  readonly usersLoading = signal(false);
  readonly filteredUsers = signal<UserLookupResponse[]>([]);

  readonly statusOptions = [
    { value: TaskItemStatus.ToDo, label: TASK_STATUS_LABELS[TaskItemStatus.ToDo] },
    { value: TaskItemStatus.InProgress, label: TASK_STATUS_LABELS[TaskItemStatus.InProgress] },
    { value: TaskItemStatus.Completed, label: TASK_STATUS_LABELS[TaskItemStatus.Completed] },
    { value: TaskItemStatus.Cancelled, label: TASK_STATUS_LABELS[TaskItemStatus.Cancelled] },
  ];

  readonly priorityOptions = [
    { value: TaskItemPriority.Low, label: TASK_PRIORITY_LABELS[TaskItemPriority.Low] },
    { value: TaskItemPriority.Medium, label: TASK_PRIORITY_LABELS[TaskItemPriority.Medium] },
    { value: TaskItemPriority.High, label: TASK_PRIORITY_LABELS[TaskItemPriority.High] },
    { value: TaskItemPriority.Critical, label: TASK_PRIORITY_LABELS[TaskItemPriority.Critical] },
  ];

  canMutate(): boolean {
    return this.authService.hasAnyRole([UserRole.Admin, UserRole.ProjectManager]);
  }

  isTeamMember(): boolean {
    return this.authService.hasRole(UserRole.TeamMember);
  }

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id');
    if (this.taskId && this.taskId !== 'create') {
      this.isEdit = true;
      this.loadTask(this.taskId);
    }
    this.loadProjects();
    this.loadUsers();
  }

  displayProject = (id: string): string => {
    const project = this.projects().find(p => p.id === id);
    return project ? project.name : '';
  };

  displayUser = (id: string): string => {
    const user = this.users().find(u => u.id === id);
    return user ? (user.fullName || user.email) : '';
  };

  private refreshDisplay(): void {
    if (!this.isEdit) return;
    const projectId = this.form.get('projectId')!.value;
    if (projectId) {
      this.form.get('projectId')!.setValue(projectId, { emitEvent: false });
    }
    const userId = this.form.get('assignedToUserId')!.value;
    if (userId) {
      this.form.get('assignedToUserId')!.setValue(userId, { emitEvent: false });
    }
  }

  loadProjects(): void {
    this.projectsLoading.set(true);
    this.projectsService.list({
      sortField: 'Name',
      sortDirection: 'Asc',
      pageNumber: 1,
      pageSize: 100,
    }).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.projectsLoading.set(false);
        if (result.success && result.data) {
          this.projects.set(result.data.items);
          this.filteredProjects.set(result.data.items);
        } else {
          this.projects.set([]);
          this.filteredProjects.set([]);
        }
        this.refreshDisplay();
      },
      error: () => {
        this.projectsLoading.set(false);
        this.projects.set([]);
        this.filteredProjects.set([]);
      }
    });
  }

  loadUsers(): void {
    this.usersLoading.set(true);
    this.usersService.list().pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        this.usersLoading.set(false);
        if (result.success && result.data) {
          this.users.set(result.data);
          this.filteredUsers.set(result.data);
        } else {
          this.users.set([]);
          this.filteredUsers.set([]);
        }
        this.refreshDisplay();
      },
      error: () => {
        this.usersLoading.set(false);
        this.users.set([]);
        this.filteredUsers.set([]);
      }
    });
  }

  loadTask(id: string): void {
    this.loading.set(true);
    this.tasksService.get(id).pipe(this.untilDestroyed).subscribe({
      next: (result) => {
        if (result.success && result.data) {
          const task = result.data;
          this.form.patchValue({
            title: task.title,
            description: task.description || '',
            status: task.status,
            priority: task.priority,
            dueDate: task.dueDate ? new Date(task.dueDate) : null,
            projectId: task.projectId,
            assignedToUserId: task.assignedToUserId || '',
          });
          this.originalAssignedToUserId = task.assignedToUserId || undefined;
        } else {
          this.notificationService.showError(result.message || 'Failed to load task');
          this.router.navigate(['/tasks']);
        }
        this.loading.set(false);
      },
      error: (err: { message?: string }) => {
        this.notificationService.showError(err.message || 'Failed to load task');
        this.router.navigate(['/tasks']);
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.loading.set(true);
    const value = this.form.value;

    const payload = {
      title: value.title,
      description: value.description || '',
      status: Number(value.status),
      priority: Number(value.priority),
      dueDate: value.dueDate ? new Date(value.dueDate).toISOString() : undefined,
      projectId: value.projectId,
      assignedToUserId: value.assignedToUserId || undefined,
    };

    if (this.isEdit && this.taskId) {
      this.tasksService.update(this.taskId, payload).pipe(this.untilDestroyed).subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.notificationService.showSuccess('Task updated successfully');
            this.form.markAsPristine();
            const newAssignee = payload.assignedToUserId || undefined;
            if (newAssignee !== this.originalAssignedToUserId) {
              this.tasksService.assign(this.taskId!, newAssignee).pipe(this.untilDestroyed).subscribe({
                next: () => this.router.navigate(['/tasks', this.taskId]),
                error: () => {
                  this.notificationService.showError('Task updated but assignment failed');
                  this.router.navigate(['/tasks', this.taskId]);
                }
              });
            } else {
              this.router.navigate(['/tasks', this.taskId]);
            }
          } else {
            this.notificationService.showError(result.message || 'Update failed');
            this.loading.set(false);
          }
        },
        error: () => {
          this.loading.set(false);
        }
      });
    } else {
      this.tasksService.create(payload.projectId, {
        title: payload.title,
        description: payload.description,
        status: payload.status,
        priority: payload.priority,
        dueDate: payload.dueDate ?? undefined,
      }).pipe(this.untilDestroyed).subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.notificationService.showSuccess('Task created successfully');
            this.form.markAsPristine();
            const newAssignee = payload.assignedToUserId || undefined;
            if (newAssignee) {
              this.tasksService.assign(result.data.id, newAssignee).pipe(this.untilDestroyed).subscribe({
                next: () => this.router.navigate(['/tasks']),
                error: () => {
                  this.notificationService.showError('Task created but assignment failed');
                  this.router.navigate(['/tasks']);
                }
              });
            } else {
              this.router.navigate(['/tasks']);
            }
          } else {
            this.notificationService.showError(result.message || 'Creation failed');
            this.loading.set(false);
          }
        },
        error: () => {
          this.loading.set(false);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }

  canDeactivate(): boolean {
    if (this.form.pristine) {
      return true;
    }
    return confirm('You have unsaved changes. Are you sure you want to leave?');
  }
}
