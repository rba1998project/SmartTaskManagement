import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { TaskResponse, UpdateTaskStatusRequest } from '../../../core/models/task';
import { TaskItemStatus } from '../../../core/models/enums';
import { TASK_STATUS_LABELS } from '../../../shared/constants/task-status.constants';
import { TextCounterPipe } from '../../../shared/pipes/text-counter.pipe';

export interface ChangeStatusDialogData {
  task: TaskResponse;
}

@Component({
  selector: 'app-change-status-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatOptionModule,
    MatSelectModule,
    TextCounterPipe,
  ],
  templateUrl: './change-status-dialog.component.html',
  styleUrl: './change-status-dialog.component.css',
})
export class ChangeStatusDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ChangeStatusDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  readonly data = inject<ChangeStatusDialogData>(MAT_DIALOG_DATA);

  readonly nextStatus = this.getNextStatus(this.data.task.status);
  readonly statusOptions = this.nextStatus === null
    ? []
    : [{ value: this.nextStatus, label: TASK_STATUS_LABELS[this.nextStatus] }];
  readonly form = this.formBuilder.nonNullable.group({
    status: [this.nextStatus, Validators.required],
    comment: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  statusLabel(status: TaskItemStatus | null): string {
    return status === null ? 'No next status' : TASK_STATUS_LABELS[status];
  }

  currentStatusLabel(): string {
    return this.statusLabel(this.normalizeStatus(this.data.task.status));
  }

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid || this.nextStatus === null) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (value.status === null) return;
    const request: UpdateTaskStatusRequest = {
      status: value.status,
      comment: value.comment.trim(),
    };
    if (!request.comment) {
      this.form.controls.comment.setErrors({ required: true });
      this.form.controls.comment.markAsTouched();
      return;
    }

    this.dialogRef.close(request);
  }

  private getNextStatus(status: TaskItemStatus): TaskItemStatus | null {
    const normalizedStatus = this.normalizeStatus(status);
    return normalizedStatus === TaskItemStatus.ToDo
      ? TaskItemStatus.InProgress
      : normalizedStatus === TaskItemStatus.InProgress
        ? TaskItemStatus.Completed
        : null;
  }

  private normalizeStatus(status: TaskItemStatus | string): TaskItemStatus {
    if (typeof status === 'number') return status;
    const byName: Record<string, TaskItemStatus> = {
      ToDo: TaskItemStatus.ToDo,
      InProgress: TaskItemStatus.InProgress,
      Completed: TaskItemStatus.Completed,
      Cancelled: TaskItemStatus.Cancelled,
    };
    return byName[status] ?? Number(status) as TaskItemStatus;
  }
}
