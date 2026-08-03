import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OperatorFunction, Subject } from 'rxjs';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs/operators';
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
import { TextCounterPipe } from '../../../shared/pipes/text-counter.pipe';

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
    TextCounterPipe,
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
    assignedToUserId: ['', Validators.required],
  });

  readonly projectSearch = new FormControl<string | ProjectResponse>('');
  readonly assigneeSearch = new FormControl<string | UserLookupResponse>('');

  loading = signal(false);
  taskId: string | null = this.route.snapshot.paramMap.get('id');
  isEdit = !!this.taskId && this.taskId !== 'create';
  originalAssignedToUserId: string | null | undefined;

  readonly projects = signal<ProjectResponse[]>([]);
  readonly projectsLoading = signal(false);
  readonly filteredProjects = signal<ProjectResponse[]>([]);
  readonly users = signal<UserLookupResponse[]>([]);
  readonly usersLoading = signal(false);
  readonly filteredUsers = signal<UserLookupResponse[]>([]);
  readonly projectHasMore = signal(false);
  readonly assigneeHasMore = signal(false);

  private projectSearchTerm = new Subject<string>();
  private assigneeSearchTerm = new Subject<string>();
  private projectPage = 1;
  private assigneePage = 1;
  private projectTerm = '';
  private assigneeTerm = '';
  private projectRequestVersion = 0;
  private assigneeRequestVersion = 0;

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
    if (this.isEdit && this.taskId) {
      this.loadTask(this.taskId);
    }
    this.setupAutocompleteSearch();
    this.loadProjects('');
    this.loadUsers('');
  }

  displayProject = (value: ProjectResponse | string | null): string => {
    return typeof value === 'object' && value ? value.name : value || '';
  };

  displayUser = (value: UserLookupResponse | string | null): string => {
    return typeof value === 'object' && value ? (value.fullName || value.email) : value || '';
  };

  private setupAutocompleteSearch(): void {
    this.projectSearch.valueChanges.pipe(this.untilDestroyed).subscribe(value => {
      if (typeof value === 'string') {
        this.form.get('projectId')!.setValue('', { emitEvent: false });
        this.projectSearchTerm.next(value);
      }
    });
    this.assigneeSearch.valueChanges.pipe(this.untilDestroyed).subscribe(value => {
      if (typeof value === 'string') {
        this.form.get('assignedToUserId')!.setValue('', { emitEvent: false });
        this.assigneeSearchTerm.next(value);
      }
    });
    this.projectSearchTerm.pipe(debounceTime(300), distinctUntilChanged(), this.untilDestroyed)
      .subscribe(term => this.loadProjects(term));
    this.assigneeSearchTerm.pipe(debounceTime(300), distinctUntilChanged(), this.untilDestroyed)
      .subscribe(term => this.loadUsers(term));
  }

  loadProjects(term: string, page = 1): void {
    const requestVersion = ++this.projectRequestVersion;
    this.projectTerm = term;
    this.projectPage = page;
    this.projectsLoading.set(true);
    this.projectsService.list({
      search: term || undefined,
      sortField: 'Name',
      sortDirection: 'Asc',
      pageNumber: page,
      pageSize: 20,
    }).pipe(this.untilDestroyed, finalize(() => this.projectsLoading.set(false))).subscribe({
      next: (result) => {
        if (requestVersion !== this.projectRequestVersion) return;
        if (result.success && result.data) {
          const items = page === 1 ? result.data.items : [...this.projects(), ...result.data.items];
          this.projects.set(items);
          this.filteredProjects.set(items);
          this.projectHasMore.set(result.data.pageNumber < result.data.totalPages);
        } else {
          this.projects.set([]);
          this.filteredProjects.set([]);
        }
      },
      error: () => {
        if (requestVersion !== this.projectRequestVersion) return;
        this.projects.set([]);
        this.filteredProjects.set([]);
      }
    });
  }

  loadUsers(term: string, page = 1): void {
    const requestVersion = ++this.assigneeRequestVersion;
    this.assigneeTerm = term;
    this.assigneePage = page;
    this.usersLoading.set(true);
    this.usersService.list({ search: term || undefined, pageNumber: page, pageSize: 20 }).pipe(
      this.untilDestroyed,
      finalize(() => this.usersLoading.set(false)),
    ).subscribe({
      next: (result) => {
        if (requestVersion !== this.assigneeRequestVersion) return;
        if (result.success && result.data) {
          const items = page === 1 ? result.data.items : [...this.users(), ...result.data.items];
          this.users.set(items);
          this.filteredUsers.set(items);
          this.assigneeHasMore.set(result.data.pageNumber < result.data.totalPages);
        } else {
          this.users.set([]);
          this.filteredUsers.set([]);
        }
      },
      error: () => {
        if (requestVersion !== this.assigneeRequestVersion) return;
        this.users.set([]);
        this.filteredUsers.set([]);
      }
    });
  }

  loadMoreProjects(): void {
    if (!this.projectsLoading() && this.projectHasMore()) this.loadProjects(this.projectTerm, this.projectPage + 1);
  }

  loadMoreUsers(): void {
    if (!this.usersLoading() && this.assigneeHasMore()) this.loadUsers(this.assigneeTerm, this.assigneePage + 1);
  }

  selectProject(event: MatAutocompleteSelectedEvent): void {
    this.form.get('projectId')!.setValue((event.option.value as ProjectResponse).id);
  }

  selectUser(event: MatAutocompleteSelectedEvent): void {
    this.form.get('assignedToUserId')!.setValue((event.option.value as UserLookupResponse).id);
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
          this.projectSearch.setValue(task.projectName, { emitEvent: false });
          this.assigneeSearch.setValue(task.assignedToUserName || '', { emitEvent: false });
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
      dueDate: value.dueDate
        ? new Date(Date.UTC(value.dueDate.getFullYear(), value.dueDate.getMonth(), value.dueDate.getDate(), 12)).toISOString()
        : undefined,
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
                next: () => this.router.navigate(['/tasks']),
                error: () => {
                  this.notificationService.showError('Task updated but assignment failed');
                  this.router.navigate(['/tasks']);
                }
              });
            } else {
              this.router.navigate(['/tasks']);
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
