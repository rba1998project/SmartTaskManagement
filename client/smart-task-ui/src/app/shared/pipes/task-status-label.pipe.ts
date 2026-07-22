import { Pipe, PipeTransform } from '@angular/core';
import { TaskItemStatus } from '../../core/models/enums';
import { TASK_STATUS_LABELS } from '../constants/task-status.constants';

@Pipe({ name: 'taskStatusLabel', standalone: true })
export class TaskStatusLabelPipe implements PipeTransform {
  transform(value: TaskItemStatus): string {
    return TASK_STATUS_LABELS[value] ?? String(value);
  }
}
