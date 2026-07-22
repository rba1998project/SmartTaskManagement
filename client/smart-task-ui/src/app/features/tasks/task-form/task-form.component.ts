import { Component, inject, signal, OnInit } from '@angular/core';
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
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TasksService } from '../../../core/services/tasks.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserRole } from '../../../core/models/enums';
import { AuthService } from '../../../core/auth/auth.service';
import { TaskItemStatus, TaskItemPriority } from '../../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../../shared/constants/task-status.constants';
import { TASK_PRIORITY_LABELS } from '../../../shared/constants/task-priority.constants';
import { AiEnhanceButtonComponent } from '../../../shared/components/ai-enhance-button/ai-enhance-button.component';

// Route: /tasks/create or /tasks/:id/edit
// Loads existing task for edit mode.
// TeamMembers get readonly title/description/priority/due-date/assigned-to fields.
// canDeactivate prevents accidental navigation with unsaved changes.
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
    AiEnhanceButtonComponent,
  ],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.css'
})
export class TaskFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tasksService = inject(TasksService);
  private notificationService = inject(NotificationService);
  private snackBar = inject(MatSnackBar);
  private authService = inject(AuthService);

  form: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    status: [TaskItemStatus.ToDo, Validators.required],
    priority: [TaskItemStatus.ToDo, Validators.required],
    dueDate: [null as string | null],
    projectId: ['', Validators.required],
    assignedToUserId: [''],
  });

  loading = signal(false);
  isEdit = false;
  taskId: string | null = null;

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
  }

  loadTask(id: string): void {
    this.loading.set(true);
    this.tasksService.get(id).subscribe({
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

  // Normalize form value into API payload; status/priority are enums stored as numbers
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
      this.tasksService.update(this.taskId, payload).subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.notificationService.showSuccess('Task updated successfully');
            this.router.navigate(['/tasks', this.taskId]);
          } else {
            this.notificationService.showError(result.message || 'Update failed');
            this.loading.set(false);
          }
        },
        error: (err: { message?: string }) => {
          this.notificationService.showError(err.message || 'Update failed');
          this.loading.set(false);
        }
      });
    } else {
      this.tasksService.create(payload.projectId, {
        title: payload.title,
        description: payload.description,
        priority: payload.priority,
        dueDate: payload.dueDate ?? undefined,
      }).subscribe({
        next: (result) => {
          if (result.success && result.data) {
            this.notificationService.showSuccess('Task created successfully');
            this.router.navigate(['/tasks', result.data.id]);
          } else {
            this.notificationService.showError(result.message || 'Creation failed');
            this.loading.set(false);
          }
        },
        error: (err: { message?: string }) => {
          this.notificationService.showError(err.message || 'Creation failed');
          this.loading.set(false);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }

  // Prompt before leaving if the form is dirty
  canDeactivate(): boolean {
    if (this.form.pristine && !this.isEdit) {
      return true;
    }
    return confirm('You have unsaved changes. Are you sure you want to leave?');
  }
}
