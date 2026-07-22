import { Pipe, PipeTransform } from '@angular/core';
import { TaskItemPriority } from '../../core/models/enums';
import { TASK_PRIORITY_LABELS } from '../constants/task-priority.constants';

@Pipe({ name: 'taskPriorityLabel', standalone: true })
export class TaskPriorityLabelPipe implements PipeTransform {
  transform(value: TaskItemPriority): string {
    return TASK_PRIORITY_LABELS[value] ?? String(value);
  }
}
